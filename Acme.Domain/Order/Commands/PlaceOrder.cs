using MediatR;

namespace Acme.Domain.Order.Commands;

public sealed record PlaceOrderItem(string Sku, int Quantity);

public sealed record PlaceOrder(
    string CustomerReference,
    string OrderType,
    IReadOnlyCollection<PlaceOrderItem> Items) : IRequest<global::Acme.Domain.Order.Order>;
