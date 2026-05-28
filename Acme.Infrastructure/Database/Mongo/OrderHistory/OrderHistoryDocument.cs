using MongoDB.Bson.Serialization.Attributes;

namespace Acme.Infrastructure.Database.Mongo.OrderHistory;

[BsonIgnoreExtraElements]
public sealed class OrderHistoryDocument
{
    public string OrderId { get; set; } = string.Empty;

    public DateTimeOffset? OrderReceivedAt { get; set; }

    public DateTimeOffset? OrderProcessedAt { get; set; }

    public DateTimeOffset? ReceiptSentAt { get; set; }

    public DateTimeOffset? OrderCompletedAt { get; set; }

    public static OrderHistoryDocument FromDomain(global::Acme.Domain.OrderHistory.OrderHistory history)
    {
        return new OrderHistoryDocument
        {
            OrderId = history.OrderId.ToString("D"),
            OrderReceivedAt = history.OrderReceivedAt,
            OrderProcessedAt = history.OrderProcessedAt,
            ReceiptSentAt = history.ReceiptSentAt,
            OrderCompletedAt = history.OrderCompletedAt
        };
    }
}
