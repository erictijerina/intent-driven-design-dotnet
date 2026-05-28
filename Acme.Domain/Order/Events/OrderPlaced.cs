using MediatR;

namespace Acme.Domain.Order.Events;

public sealed record OrderPlaced(
    Guid OrderId,
    string CustomerReference,
    string OrderType,
    OrderStatus Status,
    DateTimeOffset OccurredAt) : INotification;
