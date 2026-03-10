using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR.Client;

namespace mqonnor.StressTester;

public sealed class EventProducer(StressOptions options)
{
    // correlationId -> sentAt (Unix ms)
    public ConcurrentDictionary<Guid, long> Sent { get; } = new();
    public int Errors;

    private HubConnection? _connection;

    public async Task ConnectAsync(CancellationToken ct)
    {
        _connection = new HubConnectionBuilder()
            .WithUrl(options.Url)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<string>("Error", msg =>
            Interlocked.Increment(ref Errors));

        await _connection.StartAsync(ct);
    }

    public async Task RunAsync(CancellationToken ct)
    {
        var semaphore = new SemaphoreSlim(options.Concurrency);
        var tasks = new List<Task>(options.Events);

        for (var i = 0; i < options.Events; i++)
        {
            await semaphore.WaitAsync(ct);
            var correlationId = Guid.NewGuid();
            tasks.Add(SendOneAsync(correlationId, semaphore, ct));
        }

        await Task.WhenAll(tasks);
    }

    private async Task SendOneAsync(Guid correlationId, SemaphoreSlim semaphore, CancellationToken ct)
    {
        try
        {
            var payload = BuildPayload(correlationId);
            var sentAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            Sent[correlationId] = sentAt;

            await _connection!.InvokeAsync("Publish", new
            {
                payload,
                encoding = "application/json",
                source = "stress-tester"
            }, ct);
        }
        catch (Exception)
        {
            Interlocked.Increment(ref Errors);
        }
        finally
        {
            semaphore.Release();
        }
    }

    private byte[] BuildPayload(Guid correlationId)
    {
        using var ms = new System.IO.MemoryStream();
        using var writer = new Utf8JsonWriter(ms);
        writer.WriteStartObject();
        writer.WriteString("correlationId", correlationId);
        writer.WriteNumber("sentAt", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        if (options.ExtraPayloadBytes > 0)
            writer.WriteString("padding", new string('x', options.ExtraPayloadBytes));
        writer.WriteEndObject();
        writer.Flush();
        return ms.ToArray();
    }

    public ValueTask DisposeAsync() =>
        _connection?.DisposeAsync() ?? ValueTask.CompletedTask;
}
