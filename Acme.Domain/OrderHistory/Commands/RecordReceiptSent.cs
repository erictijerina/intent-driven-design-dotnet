using MediatR;

namespace Acme.Domain.OrderHistory.Commands;

public sealed record RecordReceiptSent(Guid OrderId, DateTimeOffset OccurredAt) : IRequest<Unit>;
