using Microsoft.Extensions.Logging;

namespace Acme.Infrastructure.Messaging.Email;

public sealed class EmailSender
{
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(ILogger<EmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendReceiptAsync(Guid orderId, CancellationToken ct = default)
    {
        _logger.LogInformation("SendReceipt for {OrderId}.", orderId);
        return Task.CompletedTask;
    }
}
