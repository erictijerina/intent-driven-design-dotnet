using Acme.Application.Contracts;
using Acme.Application.UseCases.Messaging.Outbox;
using Acme.Application.UseCases.Order.Events;
using Acme.Application.UseCases.Order.Orchestration;
using Acme.Domain.Abstractions;
using Acme.Domain.Common.Exceptions;
using Acme.Domain.Core.IntentDispatchers;
using Acme.Domain.Order;
using Acme.Domain.Order.Commands;
using Acme.Domain.Order.Events;
using Acme.Domain.Order.Workflows;
using Acme.Domain.OrderHistory.Commands;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Application.Tests.UseCases.Order;

public sealed class PlaceOrderHandlerTests
{
    [Fact]
    public async Task Handle_WithValidInput_CreatesOrder()
    {
        var store = new InMemoryOrderStore();
        var outbox = new InMemoryOutboxStore();
        var handler = new PlaceOrderHandler(store, outbox);
        var runtime = new RecordingIntentRuntime();

        IntentDispatcher.Configure(runtime);
        try
        {
            var result = await handler.Handle(
                new PlaceOrder("cust-1001", "standard", new[] { new PlaceOrderItem("sku-1", 1) }),
                CancellationToken.None);

            Assert.NotEqual(Guid.Empty, result.Id);
            Assert.Equal(OrderStatus.Pending, result.Status);
            Assert.Single(store.Orders);
            var pending = await outbox.GetPendingAsync(10);
            Assert.Single(pending);
        }
        finally
        {
            IntentDispatcher.Configure(null);
        }
    }

    [Fact]
    public async Task Handle_WithMissingCustomerReference_Throws()
    {
        var handler = new PlaceOrderHandler(new InMemoryOrderStore(), new InMemoryOutboxStore());

        await Assert.ThrowsAsync<ValidationException>(() =>
            handler.Handle(new PlaceOrder("", "standard", new[] { new PlaceOrderItem("sku-1", 1) }), CancellationToken.None));
    }
}

public sealed class ProcessOrderHandlerTests
{
    [Fact]
    public async Task Handle_WithPendingOrder_TransitionsToProcessing()
    {
        var store = new InMemoryOrderStore();
        var order = new global::Acme.Domain.Order.Order { Id = Guid.NewGuid(), Status = OrderStatus.Pending, CustomerReference = "cust", OrderType = "standard" };
        await store.AddAsync(order);
        await store.SaveChangesAsync();

        var handler = new ProcessOrderHandler(store);
        var runtime = new RecordingIntentRuntime();
        IntentDispatcher.Configure(runtime);
        try
        {
            var result = await handler.Handle(new ProcessOrder(order.Id), CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal(OrderStatus.Processing, result.Status);
        }
        finally
        {
            IntentDispatcher.Configure(null);
        }
    }
}

public sealed class OrderPlacedHandlerTests
{
    [Fact]
    public async Task Handle_DispatchesTier1Commands()
    {
        var runtime = new RecordingIntentRuntime();
        var handler = new OrderPlacedHandler(NullLogger<OrderPlacedHandler>.Instance);

        var orderId = Guid.NewGuid();
        IntentDispatcher.Configure(runtime);
        try
        {
            await handler.Handle(
                new OrderPlaced(orderId, "cust", "standard", OrderStatus.Pending, DateTimeOffset.UtcNow),
                CancellationToken.None);
        }
        finally
        {
            IntentDispatcher.Configure(null);
        }

        Assert.Collection(runtime.Intents,
            x => Assert.IsType<SendReceipt>(x),
            x => Assert.IsType<RecordOrderCompleted>(x));
    }
}

internal sealed class InMemoryOrderStore : IOrderStore
{
    public List<global::Acme.Domain.Order.Order> Orders { get; } = new();

    public Task AddAsync(global::Acme.Domain.Order.Order order, CancellationToken ct = default)
    {
        Orders.Add(order);
        return Task.CompletedTask;
    }

    public Task<global::Acme.Domain.Order.Order?> GetByIdAsync(Guid orderId, CancellationToken ct = default)
        => Task.FromResult(Orders.FirstOrDefault(x => x.Id == orderId));

    public Task SaveChangesAsync(CancellationToken ct = default) => Task.CompletedTask;
}

internal sealed class InMemoryOutboxStore : IOutboxMessageStore
{
    private readonly List<OutboxEnvelope> _messages = new();

    public Task EnqueueAsync(string type, string payload, CancellationToken ct = default)
    {
        _messages.Add(new OutboxEnvelope(Guid.NewGuid(), type, payload, DateTimeOffset.UtcNow, 0, null, null));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<OutboxEnvelope>> GetPendingAsync(int take, CancellationToken ct = default)
        => Task.FromResult((IReadOnlyList<OutboxEnvelope>)_messages.Take(take).ToList());

    public Task MarkProcessedAsync(Guid id, CancellationToken ct = default) => Task.CompletedTask;

    public Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default) => Task.CompletedTask;
}

internal sealed class RecordingIntentRuntime : IIntentRuntime
{
    public List<object> Intents { get; } = new();

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> intent, CancellationToken cancellationToken = default)
    {
        Intents.Add(intent);
        if (typeof(TResponse) == typeof(Unit))
            return Task.FromResult((TResponse)(object)Unit.Value);
        throw new InvalidOperationException($"No stubbed response for {typeof(TResponse).Name}.");
    }
}
