using Acme.Application.Contracts;
using Acme.Application.UseCases.Messaging.Outbox;
using Acme.Domain.Common.Exceptions;
using Acme.Domain.Order;
using Acme.Domain.Order.Commands;
using Acme.Domain.Order.Events;
using Acme.Domain.OrderHistory;
using Acme.Domain.OrderHistory.Commands;
using MediatR;
using System.Text.Json;

namespace Acme.Application.UseCases.Order.Orchestration;

public sealed class PlaceOrderHandler : IRequestHandler<PlaceOrder, global::Acme.Domain.Order.Order>
{
    private readonly IOrderStore _orderStore;
    private readonly IOutboxMessageStore _outbox;

    public PlaceOrderHandler(IOrderStore orderStore, IOutboxMessageStore outbox)
    {
        _orderStore = orderStore;
        _outbox = outbox;
    }

    public async Task<global::Acme.Domain.Order.Order> Handle(PlaceOrder request, CancellationToken ct)
    {        
        if (string.IsNullOrWhiteSpace(request.CustomerReference))
            throw new ValidationException("CustomerReference is required.");

        if (request.Items is null || request.Items.Count == 0)
            throw new ValidationException("At least one item is required.");

        var order = new global::Acme.Domain.Order.Order
        {
            Id = Guid.NewGuid(),
            CustomerReference = request.CustomerReference,
            OrderType = request.OrderType,
            CreatedAt = DateTimeOffset.UtcNow,
            Status = OrderStatus.Pending
        };

        await _orderStore.AddAsync(order, ct);
        await _orderStore.SaveChangesAsync(ct);

        await OrderHistory.RecordOrderReceived(
            new RecordOrderReceived(order.Id, order.CreatedAt), ct);

        var payload = JsonSerializer.Serialize(new OrderPlaced(order.Id, order.CustomerReference, order.OrderType, order.Status, order.CreatedAt));
        await _outbox.EnqueueAsync(nameof(OrderPlaced), payload, ct);

        return order;
    }
}
