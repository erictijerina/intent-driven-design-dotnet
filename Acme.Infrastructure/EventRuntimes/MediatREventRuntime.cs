using Acme.Domain.Abstractions;
using MediatR;

namespace Acme.Infrastructure.EventRuntimes;

public sealed class MediatREventRuntime(IMediator mediator) : IEventRuntime
{
    public Task RaiseAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : INotification
        => mediator.Publish(@event, cancellationToken);
}
