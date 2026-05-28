using Acme.Application.UseCases.Messaging.Outbox;
using Acme.Domain.Abstractions;
using Acme.Domain.Core.EventPublishers;
using Acme.Domain.Core.IntentDispatchers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Acme.Infrastructure.BackgroundWorkers.Messaging.Outbox;

public sealed class OutboxDispatcherBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<OutboxDispatcherBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = serviceProvider.CreateScope();
                var intentRuntime = scope.ServiceProvider.GetRequiredService<IIntentRuntime>();
                var eventRuntime = scope.ServiceProvider.GetRequiredService<IEventRuntime>();
                IntentDispatcher.Configure(intentRuntime);
                EventPublisher.Configure(eventRuntime);

                var dispatcher = scope.ServiceProvider.GetRequiredService<IOutboxDispatcher>();
                await dispatcher.DispatchPendingAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox dispatcher iteration failed.");
            }
            finally
            {
                IntentDispatcher.Configure(null);
                EventPublisher.Configure(null);
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
