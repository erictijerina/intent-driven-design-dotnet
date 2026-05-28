namespace Acme.Domain.Order.Workflows;

public sealed record ProcessOrderResult(Guid OrderId, OrderStatus Status, bool Success);
