using System.Text.Json;

namespace ProductManagement.Domain.Entities;

public sealed class OutboxMessage
{
    private OutboxMessage() { } // EF Core

    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Type { get; private set; } = default!;
    public JsonDocument Payload { get; private set; } = default!;
    public DateTime OccurredAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; private set; }

    public static OutboxMessage Create(string type, JsonDocument payload) =>
        new() {Type = type, Payload = payload};

    public void MarkProcessed() => ProcessedAt = DateTime.UtcNow;
}
