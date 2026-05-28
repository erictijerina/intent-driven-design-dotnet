using Acme.Infrastructure.BackgroundWorkers.Messaging.Outbox;
using Acme.Infrastructure.Messaging.Outbox;
using Xunit;

namespace Infrastructure.Tests.Architecture;

public sealed class DependencyRuleTests
{
    [Fact]
    public void Infrastructure_HasOutboxDispatcher()
    {
        Assert.NotNull(typeof(OutboxDispatcherBackgroundService));
        Assert.NotNull(typeof(OutboxMessageStore));
    }
}
