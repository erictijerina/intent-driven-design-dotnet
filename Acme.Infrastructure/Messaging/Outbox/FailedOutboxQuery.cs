using Acme.Application.UseCases.Messaging.Outbox;

namespace Acme.Infrastructure.Messaging.Outbox;

public sealed class FailedOutboxQuery(IOutboxMessageStore store) : IFailedOutboxQuery
{
    public async Task<IReadOnlyList<OutboxEnvelope>> GetFailedAsync(int take, CancellationToken ct = default)
    {
        var pending = await store.GetPendingAsync(take, ct);
        return pending.Where(x => !string.IsNullOrWhiteSpace(x.LastError)).ToList();
    }
}
