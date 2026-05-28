using MediatR;

namespace Acme.Domain.OrderHistory.Queries;

public sealed record GetOrderHistory(Guid OrderId) : IRequest<global::Acme.Domain.OrderHistory.OrderHistory?>;
