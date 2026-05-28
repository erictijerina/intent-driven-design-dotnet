using Acme.Domain.Abstractions;
using MediatR;

namespace Acme.Domain.Core.EventPublishers;

public static class EventPublisher
{
    private static readonly AsyncLocal<IEventRuntime?> CurrentRuntime = new();

    public static void Configure(IEventRuntime? runtime) => CurrentRuntime.Value = runtime;

    private static IEventRuntime Current => CurrentRuntime.Value ?? throw new InvalidOperationException("Event runtime is not configured.");

    public static Task RaiseAsync<TEvent>(TEvent @event, CancellationToken ct = default)
        where TEvent : INotification
        => Current.RaiseAsync(@event, ct);
}
