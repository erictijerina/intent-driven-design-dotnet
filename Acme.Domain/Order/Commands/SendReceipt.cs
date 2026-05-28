using MediatR;

namespace Acme.Domain.Order.Commands;

public sealed record SendReceipt(Guid OrderId) : IRequest<Unit>;
