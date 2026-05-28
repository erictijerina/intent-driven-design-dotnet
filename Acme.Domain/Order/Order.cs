using Acme.Domain.Core.EventPublishers;
using Acme.Domain.Core.IntentDispatchers;
using Acme.Domain.Order.Commands;
using Acme.Domain.Order.Events;
using Acme.Domain.Order.Queries;
using Acme.Domain.Order.Workflows;
using MediatR;

namespace Acme.Domain.Order;

public class Order
{
    public Guid Id { get; set; }

    public string CustomerReference { get; set; } = string.Empty;

    public string OrderType { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; set; }

    public OrderStatus Status { get; set; }

    public bool IsEligibleForProcessing() => Status == OrderStatus.Pending;

    /// <summary>
    /// Places a new order intent.
    /// </summary>
    /// <remarks>
    /// Intent: <see cref="global::Acme.Domain.Order.Commands.PlaceOrder"/>.
    /// Handler: <c>global::Acme.Application.UseCases.Order.Orchestration.PlaceOrderHandler</c>.
    /// Tier: 2 (Application Orchestration).
    /// </remarks>
    public static Task<Order> PlaceOrder(PlaceOrder intent, CancellationToken ct = default)
        => IntentDispatcher.SendAsync(intent, ct);

    /// <summary>
    /// Gets an order by id.
    /// </summary>
    /// <remarks>
    /// Intent: <see cref="global::Acme.Domain.Order.Queries.GetOrder"/>.
    /// Handler: <c>global::Acme.Infrastructure.Database.Sql.Handlers.Order.Queries.GetOrderHandler</c>.
    /// Tier: 1 (Infrastructure Query Adapter).
    /// </remarks>
    public static Task<Order?> GetOrder(GetOrder intent, CancellationToken ct = default)
        => IntentDispatcher.SendAsync(intent, ct);

    /// <summary>
    /// Executes order processing workflow.
    /// </summary>
    /// <remarks>
    /// Intent: <see cref="global::Acme.Domain.Order.Workflows.ProcessOrder"/>.
    /// Handler: <c>global::Acme.Application.UseCases.Order.Orchestration.ProcessOrderHandler</c>.
    /// Tier: 2 (Application Orchestration).
    /// </remarks>
    public static Task<ProcessOrderResult> ProcessOrder(ProcessOrder intent, CancellationToken ct = default)
        => IntentDispatcher.SendAsync(intent, ct);

    /// <summary>
    /// Sends an order receipt.
    /// </summary>
    /// <remarks>
    /// Intent: <see cref="global::Acme.Domain.Order.Commands.SendReceipt"/>.
    /// Handler: <c>global::Acme.Infrastructure.Messaging.Email.Handlers.SendReceiptHandler</c>.
    /// Tier: 1 (Infrastructure Email Adapter).
    /// </remarks>
    public static Task SendReceipt(SendReceipt intent, CancellationToken ct = default)
        => IntentDispatcher.SendAsync<Unit>(intent, ct);

    /// <summary>
    /// Raises the order placed domain event.
    /// </summary>
    /// <remarks>
    /// Event: <see cref="global::Acme.Domain.Order.Events.OrderPlaced"/>.
    /// Handler: <c>global::Acme.Application.UseCases.Order.Events.OrderPlacedHandler</c>.
    /// Tier: Event Notification (Application Event Handlers).
    /// </remarks>
    public static Task OrderPlaced(OrderPlaced @event, CancellationToken ct = default)
        => EventPublisher.RaiseAsync(@event, ct);
}
