namespace Acme.Application.Dtos.OrderHistory;

public sealed record OrderHistoryResponse(
    Guid OrderId,
    DateTimeOffset? OrderReceivedAt,
    DateTimeOffset? OrderProcessedAt,
    DateTimeOffset? ReceiptSentAt,
    DateTimeOffset? OrderCompletedAt);
