namespace Acme.Application.UseCases.Messaging.Outbox;

public interface IFailedOutboxQuery
{
    Task<IReadOnlyList<OutboxEnvelope>> GetFailedAsync(int take, CancellationToken ct = default);
}
