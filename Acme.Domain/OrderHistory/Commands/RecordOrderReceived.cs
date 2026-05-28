using MediatR;

namespace Acme.Domain.OrderHistory.Commands;

public sealed record RecordOrderReceived(Guid OrderId, DateTimeOffset OccurredAt) : IRequest<Unit>;
