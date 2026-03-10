using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace mqonnor.StressTester;

public sealed class EventConsumer(StressOptions options)
{
    // correlationId -> receivedAt (Unix ms)
    public ConcurrentDictionary<Guid, long> Received { get; } = new();

    private HubConnection? _connection;

    public async Task ConnectAsync(CancellationToken ct)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(options.Url)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<JsonElement>("Notify", dto =>
        {
            var receivedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            try
            {
                // BroadcastEventDto has a .payload JsonElement containing our sentAt/correlationId
                var payload = dto.GetProperty("payload");
                if (payload.TryGetProperty("correlationId", out var cidEl) &&
                    cidEl.TryGetGuid(out var correlationId))
                {
                    Received[correlationId] = receivedAt;
                }
            }
            catch
            {
                // Non-stress-tester event or unexpected shape — ignore
            }
        });

        _connection.On<JsonElement>("NotifyBatch", batch =>
        {
            var receivedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            foreach (var dto in batch.EnumerateArray())
            {
                try
                {
                    var payload = dto.GetProperty("payload");
                    if (payload.TryGetProperty("correlationId", out var cidEl) &&
                        cidEl.TryGetGuid(out var correlationId))
                    {
                        Received[correlationId] = receivedAt;
                    }
                }
                catch
                {
                    // Non-stress-tester event or unexpected shape — ignore
                }
            }
        });

        await _connection.StartAsync(ct);
    }

    public ValueTask DisposeAsync() =>
        _connection?.DisposeAsync() ?? ValueTask.CompletedTask;
}
