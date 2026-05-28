using MediatR;

namespace Acme.Domain.Order.Queries;

public sealed record GetOrder(Guid OrderId) : IRequest<global::Acme.Domain.Order.Order?>;
