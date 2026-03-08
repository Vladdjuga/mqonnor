using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using mqonnor.Application.Messaging;
using mqonnor.Domain.Entities;
using mqonnor.Domain.Repositories;

namespace mqonnor.Infra.Workers;

public sealed class EventConsumerWorker(
    IEventBus eventBus,
    ILogger<EventConsumerWorker> logger,
    IEventRepository repository) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("EventConsumerWorker started.");
        try
        {
            await foreach (var @event in eventBus.ConsumeAllAsync(stoppingToken))
            {
                try
                {
                    await ProcessAsync(@event, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to process event {EventId}.", @event.Id);
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("EventConsumerWorker stopping — cancellation requested.");
        }

        logger.LogInformation("EventConsumerWorker stopped.");
    }

    private async Task ProcessAsync(Event @event, CancellationToken cancellationToken)
    {
        await repository.AddAsync(@event, cancellationToken);
    }
}
