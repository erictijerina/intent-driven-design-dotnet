using MediatR;

namespace Acme.Domain.OrderHistory.Commands;

public sealed record RecordOrderCompleted(Guid OrderId, DateTimeOffset OccurredAt) : IRequest<Unit>;
