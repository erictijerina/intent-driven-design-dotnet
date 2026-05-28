using Acme.Domain.Core.IntentDispatchers;
using Acme.Domain.OrderHistory.Commands;
using Acme.Domain.OrderHistory.Queries;
using MediatR;

namespace Acme.Domain.OrderHistory;

public sealed class OrderHistory
{
    public Guid OrderId { get; set; }

    public DateTimeOffset? OrderReceivedAt { get; private set; }

    public DateTimeOffset? OrderProcessedAt { get; private set; }

    public DateTimeOffset? ReceiptSentAt { get; private set; }

    public DateTimeOffset? OrderCompletedAt { get; private set; }

    public static OrderHistory FromSnapshot(
        Guid orderId,
        DateTimeOffset? orderReceivedAt,
        DateTimeOffset? orderProcessedAt,
        DateTimeOffset? receiptSentAt,
        DateTimeOffset? orderCompletedAt)
    {
        return new OrderHistory
        {
            OrderId = orderId,
            OrderReceivedAt = orderReceivedAt,
            OrderProcessedAt = orderProcessedAt,
            ReceiptSentAt = receiptSentAt,
            OrderCompletedAt = orderCompletedAt
        };
    }

    /// <summary>
    /// Records that the order was received.
    /// </summary>
    /// <remarks>
    /// Intent: <see cref="global::Acme.Domain.OrderHistory.Commands.RecordOrderReceived"/>.
    /// Handler: <c>global::Acme.Infrastructure.Database.Mongo.Handlers.OrderHistory.Commands.RecordOrderReceivedHandler</c>.
    /// Tier: 1 (Infrastructure Mongo Command Adapter).
    /// </remarks>
    public static Task RecordOrderReceived(RecordOrderReceived intent, CancellationToken ct = default)
        => IntentDispatcher.SendAsync<Unit>(intent, ct);

    /// <summary>
    /// Records that the order entered processing.
    /// </summary>
    /// <remarks>
    /// Intent: <see cref="global::Acme.Domain.OrderHistory.Commands.RecordOrderProcessed"/>.
    /// Handler: <c>global::Acme.Infrastructure.Database.Mongo.Handlers.OrderHistory.Commands.RecordOrderProcessedHandler</c>.
    /// Tier: 1 (Infrastructure Mongo Command Adapter).
    /// </remarks>
    public static Task RecordOrderProcessed(RecordOrderProcessed intent, CancellationToken ct = default)
        => IntentDispatcher.SendAsync<Unit>(intent, ct);

    /// <summary>
    /// Records that the order receipt was sent.
    /// </summary>
    /// <remarks>
    /// Intent: <see cref="global::Acme.Domain.OrderHistory.Commands.RecordReceiptSent"/>.
    /// Handler: <c>global::Acme.Infrastructure.Database.Mongo.Handlers.OrderHistory.Commands.RecordReceiptSentHandler</c>.
    /// Tier: 1 (Infrastructure Mongo Command Adapter).
    /// </remarks>
    public static Task RecordReceiptSent(RecordReceiptSent intent, CancellationToken ct = default)
        => IntentDispatcher.SendAsync<Unit>(intent, ct);

    /// <summary>
    /// Records that the order completed.
    /// </summary>
    /// <remarks>
    /// Intent: <see cref="global::Acme.Domain.OrderHistory.Commands.RecordOrderCompleted"/>.
    /// Handler: <c>global::Acme.Infrastructure.Database.Mongo.Handlers.OrderHistory.Commands.RecordOrderCompletedHandler</c>.
    /// Tier: 1 (Infrastructure Mongo Command Adapter).
    /// </remarks>
    public static Task RecordOrderCompleted(RecordOrderCompleted intent, CancellationToken ct = default)
        => IntentDispatcher.SendAsync<Unit>(intent, ct);

    /// <summary>
    /// Gets order history by order id.
    /// </summary>
    /// <remarks>
    /// Intent: <see cref="global::Acme.Domain.OrderHistory.Queries.GetOrderHistory"/>.
    /// Handler: <c>global::Acme.Infrastructure.Database.Mongo.Handlers.OrderHistory.Queries.GetOrderHistoryHandler</c>.
    /// Tier: 1 (Infrastructure Mongo Query Adapter).
    /// </remarks>
    public static Task<OrderHistory?> GetOrderHistory(GetOrderHistory intent, CancellationToken ct = default)
        => IntentDispatcher.SendAsync<OrderHistory?>(intent, ct);
}
