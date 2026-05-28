using Acme.Application.UseCases.Messaging.Outbox;
using Acme.Domain.Order.Events;
using Acme.Infrastructure.Database.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Acme.Infrastructure.Messaging.Outbox;

public sealed class OutboxMessageStore(AcmeDbContext dbContext, ILogger<OutboxMessageStore> logger)
    : IOutboxMessageStore, IOutboxDispatcher
{
    public async Task EnqueueAsync(string type, string payload, CancellationToken ct = default)
    {
        dbContext.OutboxMessages.Add(new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = type,
            Payload = payload,
            OccurredAt = DateTimeOffset.UtcNow
        });

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<OutboxEnvelope>> GetPendingAsync(int take, CancellationToken ct = default)
    {
        return await dbContext.OutboxMessages
            .Where(x => x.ProcessedAt == null)
            .OrderBy(x => x.Id)
            .Take(take)
            .Select(x => new OutboxEnvelope(x.Id, x.Type, x.Payload, x.OccurredAt, x.Attempts, x.ProcessedAt, x.LastError))
            .ToListAsync(ct);
    }

    public async Task MarkProcessedAsync(Guid id, CancellationToken ct = default)
    {
        var message = await dbContext.OutboxMessages.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (message is null)
            return;

        message.ProcessedAt = DateTimeOffset.UtcNow;
        message.LastError = null;
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task MarkFailedAsync(Guid id, string error, CancellationToken ct = default)
    {
        var message = await dbContext.OutboxMessages.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (message is null)
            return;

        message.Attempts += 1;
        message.LastError = error;
        await dbContext.SaveChangesAsync(ct);
    }

    public async Task<int> DispatchPendingAsync(CancellationToken ct = default)
    {
        var pending = await GetPendingAsync(50, ct);
        foreach (var item in pending)
        {
            try
            {
                logger.LogInformation("Dispatching outbox message {Id} of type {Type}.", item.Id, item.Type);
                if (item.Type == nameof(OrderPlaced))
                {
                    var @event = JsonSerializer.Deserialize<OrderPlaced>(item.Payload)
                        ?? throw new InvalidOperationException("Invalid OrderPlaced payload.");

                    await Domain.Order.Order.OrderPlaced(@event, ct);
                    await MarkProcessedAsync(item.Id, ct);
                    continue;
                }

                await MarkFailedAsync(item.Id, $"Unsupported outbox type: {item.Type}", ct);
            }
            catch (Exception ex)
            {
                await MarkFailedAsync(item.Id, ex.Message, ct);
            }
        }

        return pending.Count;
    }
}
