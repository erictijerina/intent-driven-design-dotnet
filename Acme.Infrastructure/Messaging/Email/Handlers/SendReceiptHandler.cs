using Acme.Domain.Order.Commands;
using Acme.Domain.OrderHistory;
using Acme.Domain.OrderHistory.Commands;
using MediatR;

namespace Acme.Infrastructure.Messaging.Email.Handlers;

public sealed class SendReceiptHandler : IRequestHandler<SendReceipt, Unit>
{
    private readonly EmailSender _emailSender;

    public SendReceiptHandler(EmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task<Unit> Handle(SendReceipt request, CancellationToken ct)
    {
        await _emailSender.SendReceiptAsync(request.OrderId, ct);

        await OrderHistory.RecordReceiptSent(
            new RecordReceiptSent(request.OrderId, DateTimeOffset.UtcNow), ct);

        return Unit.Value;
    }
}
