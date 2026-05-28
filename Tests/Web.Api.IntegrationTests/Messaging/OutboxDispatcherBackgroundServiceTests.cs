using Acme.Application.UseCases.Messaging.Outbox;
using Acme.Infrastructure.BackgroundWorkers.Messaging.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Web.Api.IntegrationTests.Messaging;

public sealed class OutboxDispatcherBackgroundServiceTests
{
    [Fact]
    public void CanCreateBackgroundService()
    {
        var services = new ServiceCollection();
        services.AddScoped<IOutboxDispatcher, StubOutboxDispatcher>();

        var provider = services.BuildServiceProvider();
        var service = new OutboxDispatcherBackgroundService(provider, NullLogger<OutboxDispatcherBackgroundService>.Instance);

        Assert.NotNull(service);
    }

    private sealed class StubOutboxDispatcher : IOutboxDispatcher
    {
        public Task<int> DispatchPendingAsync(CancellationToken ct = default) => Task.FromResult(0);
    }
}
