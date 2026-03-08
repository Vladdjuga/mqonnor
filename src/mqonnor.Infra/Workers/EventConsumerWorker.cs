using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using mqonnor.Application.Messaging;
using mqonnor.Domain.Entities;

namespace mqonnor.Infra.Workers;

public sealed class EventConsumerWorker(
    IEventBus eventBus,
    ILogger<EventConsumerWorker> logger) : BackgroundService
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

    private static Task ProcessAsync(Event @event, CancellationToken cancellationToken)
    {
        // TODO: dispatch event to the appropriate handler
        return Task.CompletedTask;
    }
}
