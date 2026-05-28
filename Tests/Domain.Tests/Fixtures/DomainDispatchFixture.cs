using Acme.Domain.Abstractions;
using Acme.Domain.Core.EventPublishers;
using Acme.Domain.Core.IntentDispatchers;
using MediatR;

namespace Domain.Tests.Fixtures;

public static class DomainDispatchFixture
{
    public static void Configure(IIntentRuntime intentRuntime, IEventRuntime eventRuntime)
    {
        IntentDispatcher.Configure(intentRuntime);
        EventPublisher.Configure(eventRuntime);
    }

    public static void Reset()
    {
        IntentDispatcher.Configure(null);
        EventPublisher.Configure(null);
    }
}

public sealed class FakeIntentRuntime(Func<object, object?> callback) : IIntentRuntime
{
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> intent, CancellationToken cancellationToken = default)
        => Task.FromResult((TResponse)callback(intent)!);
}

public sealed class FakeEventRuntime(Action<INotification> callback) : IEventRuntime
{
    public Task RaiseAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : INotification
    {
        callback(@event);
        return Task.CompletedTask;
    }
}
