using MediatR;

namespace Acme.Domain.Abstractions;

public interface IEventRuntime
{
    Task RaiseAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : INotification;
}
