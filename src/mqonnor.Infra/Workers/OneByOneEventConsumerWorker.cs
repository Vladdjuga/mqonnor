using Microsoft.Extensions.Logging;
using mqonnor.Application.Messaging;
using mqonnor.Domain.Repositories;

namespace mqonnor.Infra.Workers;

public sealed class OneByOneEventConsumerWorker(
    IEventBus eventBus,
    ILogger<EventConsumerWorker> logger,
    IEventRepository repository) : EventConsumerWorker(eventBus, logger, repository)
{
    protected override async Task RunLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var @event = await EventBus.ConsumeAsync(stoppingToken);
                await ProcessAsync(@event, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to process event.");
            }
        }
    }
}
