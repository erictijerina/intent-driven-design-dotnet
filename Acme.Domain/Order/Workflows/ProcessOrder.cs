using MediatR;

namespace Acme.Domain.Order.Workflows;

public sealed record ProcessOrder(Guid OrderId) : IRequest<ProcessOrderResult>;
