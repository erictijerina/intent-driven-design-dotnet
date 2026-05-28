namespace Acme.Application.UseCases.Messaging.Outbox;

public interface IOutboxDispatcher
{
    Task<int> DispatchPendingAsync(CancellationToken ct = default);
}
