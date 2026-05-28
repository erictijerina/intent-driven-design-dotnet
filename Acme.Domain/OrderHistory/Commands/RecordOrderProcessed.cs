using MediatR;

namespace Acme.Domain.OrderHistory.Commands;

public sealed record RecordOrderProcessed(Guid OrderId, DateTimeOffset OccurredAt) : IRequest<Unit>;
