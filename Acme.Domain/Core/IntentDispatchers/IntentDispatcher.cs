using Acme.Domain.Abstractions;
using MediatR;

namespace Acme.Domain.Core.IntentDispatchers;

public static class IntentDispatcher
{
    private static readonly AsyncLocal<IIntentRuntime?> CurrentRuntime = new();

    public static void Configure(IIntentRuntime? runtime) => CurrentRuntime.Value = runtime;

    private static IIntentRuntime Current => CurrentRuntime.Value ?? throw new InvalidOperationException("Intent runtime is not configured.");

    public static Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> intent, CancellationToken ct = default)
        => Current.SendAsync(intent, ct);
}
