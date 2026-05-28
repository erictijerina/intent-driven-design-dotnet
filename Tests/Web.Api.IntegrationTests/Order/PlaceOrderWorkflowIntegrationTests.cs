using Acme.Application.UseCases.Messaging.Outbox;
using Acme.Domain.Abstractions;
using Acme.Domain.Core.EventPublishers;
using Acme.Domain.Core.IntentDispatchers;
using Acme.Domain.Order.Events;
using Acme.Infrastructure.Database.Mongo.OrderHistory;
using Acme.Infrastructure.Database.Sql;
using MediatR;
using Mongo2Go;
using MongoDB.Driver;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Web.Api.IntegrationTests.Order;

public sealed class PlaceOrderWorkflowIntegrationTests
{
    [Fact]
    public async Task PlaceOrder_SpinsUpApi_PersistsSqlAndMongoState()
    {
        var dbFile = Path.Combine(Path.GetTempPath(), $"acme-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={dbFile}";
        using var mongoRunner = MongoDbRunner.Start(singleNodeReplSet: true);

        await using var factory = new Infrastructure.ApiWebApplicationFactory(
            connectionString,
            mongoRunner.ConnectionString);
        using var client = factory.CreateClient();

        var payload = new
        {
            customerReference = "cust-1001",
            orderType = "standard",
            items = new[] { new { sku = "sku-1", quantity = 2 } }
        };

        var response = await client.PostAsJsonAsync("/api/order", payload);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AcmeDbContext>();
        Assert.True(await db.Orders.AnyAsync());
        Assert.True(await db.OutboxMessages.AnyAsync());

        var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>();
        var handlers = scope.ServiceProvider.GetServices<INotificationHandler<OrderPlaced>>();
        Assert.NotEmpty(handlers);
        var intentRuntime = scope.ServiceProvider.GetRequiredService<IIntentRuntime>();
        var eventRuntime = scope.ServiceProvider.GetRequiredService<IEventRuntime>();
        IntentDispatcher.Configure(intentRuntime);
        EventPublisher.Configure(eventRuntime);

        try
        {
            var dispatched = await dispatcher.DispatchPendingAsync();
            Assert.True(dispatched > 0);
        }
        finally
        {
            IntentDispatcher.Configure(null);
            EventPublisher.Configure(null);
        }

        var outboxMessage = await db.OutboxMessages.FirstAsync();
        Assert.Null(outboxMessage.LastError);
        Assert.NotNull(outboxMessage.ProcessedAt);

        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();
        var histories = database.GetCollection<OrderHistoryDocument>("order_history");

        OrderHistoryDocument? history = null;
        for (var i = 0; i < 20 && history is null; i++)
        {
            history = await histories.Find(x => true).FirstOrDefaultAsync();
            if (history is null)
                await Task.Delay(100);
        }

        Assert.NotNull(history);
        Assert.True(Guid.TryParse(history!.OrderId, out var parsedOrderId));
        Assert.NotEqual(Guid.Empty, parsedOrderId);
        Assert.NotNull(history.OrderReceivedAt);
        Assert.NotNull(history.OrderProcessedAt);
        Assert.NotNull(history.ReceiptSentAt);
        Assert.NotNull(history.OrderCompletedAt);
    }
}
