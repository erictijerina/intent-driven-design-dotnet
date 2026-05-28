namespace Acme.Application.UseCases.Messaging.Outbox;

public interface IOutboxMessageStore
{
    Task EnqueueAsync(string type, string payload, CancellationToken ct = default);

    Task<IReadOnlyList<OutboxEnvelope>> GetPendingAsync(int take, CancellationToken ct = default);

    Task MarkProcessedAsync(Guid id, CancellationToken ct = default);

    Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default);
}
