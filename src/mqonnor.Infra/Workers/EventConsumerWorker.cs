using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using mqonnor.Application.Messaging;
using mqonnor.Application.Services;
using mqonnor.Domain.Entities;
using mqonnor.Domain.Repositories;

namespace mqonnor.Infra.Workers;

public abstract class EventConsumerWorker(
    IEventBus eventBus,
    ILogger<EventConsumerWorker> logger,
    IEventRepository repository,
    INotificationService notificationService) : BackgroundService
{
    protected IEventBus EventBus { get; } = eventBus;
    protected ILogger<EventConsumerWorker> Logger { get; } = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("{Worker} started.", GetType().Name);
        try
        {
            await RunLoopAsync(stoppingToken);
        }
        catch (OperationCanceledException)
        {
            Logger.LogInformation("{Worker} stopping — cancellation requested.", GetType().Name);
        }

        Logger.LogInformation("{Worker} stopped.", GetType().Name);
    }

    protected abstract Task RunLoopAsync(CancellationToken stoppingToken);

    protected async Task ProcessAsync(Event @event, CancellationToken cancellationToken)
    {
        await repository.AddAsync(@event, cancellationToken);
        await notificationService.BroadcastEventAsync(@event, cancellationToken);
    }

    protected async Task ProcessManyAsync(IEnumerable<Event> events, CancellationToken cancellationToken)
    {
        var eventList = events as IReadOnlyList<Event> ?? events.ToList();
        await repository.AddManyAsync(eventList, cancellationToken);
        foreach (var @event in eventList)
            await notificationService.BroadcastEventAsync(@event, cancellationToken);
    }
}
