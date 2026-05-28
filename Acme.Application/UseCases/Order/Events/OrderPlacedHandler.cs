using Acme.Domain.Order.Events;
using Acme.Domain.Order.Commands;
using Acme.Domain.OrderHistory;
using Acme.Domain.OrderHistory.Commands;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Acme.Application.UseCases.Order.Events;

public sealed class OrderPlacedHandler : INotificationHandler<OrderPlaced>
{
    private readonly ILogger<OrderPlacedHandler> _logger;

    public OrderPlacedHandler(ILogger<OrderPlacedHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(OrderPlaced notification, CancellationToken ct)
    {
        _logger.LogInformation("OrderPlaced for {OrderId}.", notification.OrderId);

        await global::Acme.Domain.Order.Order.SendReceipt(
            new SendReceipt(notification.OrderId), ct);
        
        await OrderHistory.RecordOrderCompleted(
            new RecordOrderCompleted(notification.OrderId, DateTimeOffset.UtcNow), ct);
    }
}
