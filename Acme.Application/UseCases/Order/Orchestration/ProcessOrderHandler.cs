using Acme.Application.Contracts;
using Acme.Domain.Common.Exceptions;
using Acme.Domain.Order;
using Acme.Domain.Order.Workflows;
using Acme.Domain.OrderHistory;
using Acme.Domain.OrderHistory.Commands;
using MediatR;

namespace Acme.Application.UseCases.Order.Orchestration;

public sealed class ProcessOrderHandler : IRequestHandler<ProcessOrder, ProcessOrderResult>
{
    private readonly IOrderStore _orderStore;

    public ProcessOrderHandler(IOrderStore orderStore)
    {
        _orderStore = orderStore;
    }

    public async Task<ProcessOrderResult> Handle(ProcessOrder request, CancellationToken ct)
    {
        var order = await _orderStore.GetByIdAsync(request.OrderId, ct);
        if (order is null)
            throw new NotFoundException($"Order {request.OrderId} not found.");

        if (!order.IsEligibleForProcessing())
            throw new DomainException($"Order {request.OrderId} is not eligible for processing.");

        order.Status = OrderStatus.Processing;
        
        await OrderHistory.RecordOrderProcessed(
            new RecordOrderProcessed(order.Id, DateTimeOffset.UtcNow), ct);

        await _orderStore.SaveChangesAsync(ct);

        return new ProcessOrderResult(order.Id, order.Status, true);
    }
}
