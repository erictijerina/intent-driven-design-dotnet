namespace Acme.Application.UseCases.Messaging.Outbox;

public sealed record OutboxEnvelope(
    Guid Id,
    string Type,
    string Payload,
    DateTimeOffset OccurredAt,
    int Attempts,
    DateTimeOffset? ProcessedAt,
    string? LastError);
