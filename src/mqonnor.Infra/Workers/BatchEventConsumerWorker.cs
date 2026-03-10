using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using mqonnor.Application.Messaging;
using mqonnor.Application.Services;
using mqonnor.Domain.Repositories;

namespace mqonnor.Infra.Workers;

public sealed class BatchEventConsumerWorker(
    IEventBus eventBus,
    ILogger<EventConsumerWorker> logger,
    IEventRepository repository,
    INotificationService notificationService,
    IOptions<EventConsumerOptions> options) : EventConsumerWorker(eventBus, logger, repository, notificationService)
{
    private readonly int _batchSize = options.Value.BatchSize;

    protected override async Task RunLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var events = await EventBus.ConsumeBatchAsync(_batchSize, stoppingToken);
                await ProcessManyAsync(events, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to process event batch.");
            }
        }
    }
}
