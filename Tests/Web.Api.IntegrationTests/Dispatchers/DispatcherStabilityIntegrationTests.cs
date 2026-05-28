using Acme.Application.Dtos.Order;
using Acme.Application.Dtos.OrderHistory;
using Acme.Application.UseCases.Messaging.Outbox;
using Acme.Domain.Abstractions;
using Acme.Domain.Core.EventPublishers;
using Acme.Domain.Core.IntentDispatchers;
using Acme.Domain.Order.Events;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using System.Net;
using System.Net.Http.Json;
using Web.Api.IntegrationTests.Infrastructure;

namespace Web.Api.IntegrationTests.Dispatchers;

public sealed class DispatcherStabilityIntegrationTests
{
    [Fact]
    public async Task SequentialWorkflowRuns_RemainStableAcrossReplayCycles()
    {
        var dbFile = Path.Combine(Path.GetTempPath(), $"acme-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={dbFile}";
        using var mongoRunner = MongoDbRunner.Start(singleNodeReplSet: true);

        await using var factory = new ApiWebApplicationFactory(connectionString, mongoRunner.ConnectionString);
        using var client = factory.CreateClient();

        var orderIds = new List<Guid>();
        for (var i = 0; i < 4; i++)
        {
            var order = await PlaceOrderAsync(client, $"stable-seq-{i}-{Guid.NewGuid():N}");
            await ProcessOrderAsync(client, order.Id);
            orderIds.Add(order.Id);
        }

        var dispatched = await ReplayOutboxAsync(factory);
        Assert.True(dispatched >= orderIds.Count);

        foreach (var orderId in orderIds)
        {
            var history = await WaitForOrderHistoryAsync(client, orderId);
            Assert.NotNull(history.OrderReceivedAt);
            Assert.NotNull(history.OrderProcessedAt);
            Assert.NotNull(history.ReceiptSentAt);
            Assert.NotNull(history.OrderCompletedAt);
        }

        var secondDispatch = await ReplayOutboxAsync(factory);
        Assert.Equal(0, secondDispatch);
    }

    [Fact]
    public async Task ParallelWorkflowRuns_StayIsolated_WhenDispatchersAreConfiguredPerScope()
    {
        var dbFile = Path.Combine(Path.GetTempPath(), $"acme-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={dbFile}";
        using var mongoRunner = MongoDbRunner.Start(singleNodeReplSet: true);

        await using var factory = new ApiWebApplicationFactory(connectionString, mongoRunner.ConnectionString);
        using var client = factory.CreateClient();

        var placements = Enumerable.Range(0, 8)
            .Select(i => PlaceOrderAsync(client, $"stable-par-{i}-{Guid.NewGuid():N}"));
        var orders = await Task.WhenAll(placements);
        await Task.WhenAll(orders.Select(order => ProcessOrderAsync(client, order.Id)));

        var dispatched = await ReplayOutboxAsync(factory);
        Assert.True(dispatched >= orders.Length);

        foreach (var order in orders)
        {
            var history = await WaitForOrderHistoryAsync(client, order.Id);
            Assert.Equal(order.Id, history.OrderId);
            Assert.NotNull(history.OrderCompletedAt);
        }
    }

    [Fact]
    public async Task ReplayOutbox_ClearsStaticDispatchers_AfterCompletion()
    {
        var dbFile = Path.Combine(Path.GetTempPath(), $"acme-{Guid.NewGuid():N}.db");
        var connectionString = $"Data Source={dbFile}";
        using var mongoRunner = MongoDbRunner.Start(singleNodeReplSet: true);

        await using var factory = new ApiWebApplicationFactory(connectionString, mongoRunner.ConnectionString);
        using var client = factory.CreateClient();

        var order = await PlaceOrderAsync(client, $"stable-clear-{Guid.NewGuid():N}");
        var dispatched = await ReplayOutboxAsync(factory);
        Assert.True(dispatched > 0);

        var history = await WaitForOrderHistoryAsync(client, order.Id);
        Assert.NotNull(history.OrderCompletedAt);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            global::Acme.Domain.Order.Order.OrderPlaced(new global::Acme.Domain.Order.Events.OrderPlaced(
                Guid.NewGuid(),
                "cleanup-check",
                "standard",
                global::Acme.Domain.Order.OrderStatus.Pending,
                DateTimeOffset.UtcNow)));
    }

    private static async Task<OrderResponse> PlaceOrderAsync(HttpClient client, string customerReference)
    {
        var payload = new
        {
            customerReference,
            orderType = "standard",
            items = new[] { new { sku = "sku-1", quantity = 1 } }
        };

        var response = await client.PostAsJsonAsync("/api/order", payload);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(order);
        Assert.NotEqual(Guid.Empty, order!.Id);
        return order;
    }

    private static async Task<int> ReplayOutboxAsync(ApiWebApplicationFactory factory)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>();
        var intentRuntime = scope.ServiceProvider.GetRequiredService<IIntentRuntime>();
        var eventRuntime = scope.ServiceProvider.GetRequiredService<IEventRuntime>();
        IntentDispatcher.Configure(intentRuntime);
        EventPublisher.Configure(eventRuntime);

        try
        {
            return await dispatcher.DispatchPendingAsync();
        }
        finally
        {
            IntentDispatcher.Configure(null);
            EventPublisher.Configure(null);
        }
    }

    private static async Task ProcessOrderAsync(HttpClient client, Guid orderId)
    {
        var response = await client.PostAsync($"/api/order/{orderId}/process", content: null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<OrderHistoryResponse> WaitForOrderHistoryAsync(HttpClient client, Guid orderId)
    {
        for (var i = 0; i < 30; i++)
        {
            var response = await client.GetAsync($"/api/orderhistory/{orderId}");
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var history = await response.Content.ReadFromJsonAsync<OrderHistoryResponse>();
                if (history is not null)
                    return history;
            }

            await Task.Delay(100);
        }

        throw new Xunit.Sdk.XunitException($"Order history not found for order {orderId}.");
    }
}
