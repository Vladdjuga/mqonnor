using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using mqonnor.Application.Messaging;
using mqonnor.Application.Services;
using mqonnor.Domain.Entities;
using mqonnor.Domain.Repositories;

namespace mqonnor.Infra.Workers;

public sealed class AllEventConsumerWorker(
    IEventBus eventBus,
    ILogger<EventConsumerWorker> logger,
    IEventRepository repository,
    INotificationService notificationService,
    Channel<IReadOnlyList<Event>> dbChannel) : EventConsumerWorker(eventBus, logger, repository, notificationService, dbChannel)
{
    protected override async Task RunLoopAsync(CancellationToken stoppingToken)
    {
        await foreach (var @event in EventBus.ConsumeAllAsync(stoppingToken))
        {
            try
            {
                await ProcessAsync(@event, stoppingToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to process event {EventId}.", @event.Id);
            }
        }
    }
}
