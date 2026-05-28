using MediatR;

namespace Acme.Domain.Abstractions;

public interface IIntentRuntime
{
    Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> intent, CancellationToken cancellationToken = default);
}
