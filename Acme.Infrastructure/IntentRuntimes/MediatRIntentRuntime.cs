using Acme.Domain.Abstractions;
using MediatR;

namespace Acme.Infrastructure.IntentRuntimes;

public sealed class MediatRIntentRuntime(IMediator mediator) : IIntentRuntime
{
    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> intent, CancellationToken cancellationToken = default)
        => mediator.Send(intent, cancellationToken);
}
