using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using mqonnor.Domain.Entities;
using mqonnor.Domain.Repositories;

namespace mqonnor.Infra.Workers;

/// <summary>
/// Drains the write-behind DB channel independently of the broadcast workers.
/// MongoDB checkpoint stalls only affect this worker — broadcast latency is unaffected.
/// </summary>
public sealed class DbPersistenceWorker(
    Channel<IReadOnlyList<Event>> dbChannel,
    IEventRepository repository,
    ILogger<DbPersistenceWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("DbPersistenceWorker started.");
        try
        {
            await foreach (var batch in dbChannel.Reader.ReadAllAsync(stoppingToken))
            {
                try
                {
                    await repository.AddManyAsync(batch, stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "[DB] Failed to persist batch of {Count} events.", batch.Count);
                }
            }
        }
        catch (OperationCanceledException) { }

        logger.LogInformation("DbPersistenceWorker stopped.");
    }
}
