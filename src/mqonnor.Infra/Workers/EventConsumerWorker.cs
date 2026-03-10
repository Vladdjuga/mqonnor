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
        Logger.LogDebug("[DB] Event {EventId} saved — source={Source} encoding={Encoding} bytes={Bytes}",
            @event.Id, @event.Metainfo.Source, @event.Metainfo.Encoding, @event.Metainfo.Length);
        await notificationService.BroadcastEventAsync(@event, cancellationToken);
        Logger.LogDebug("[SignalR] Event {EventId} broadcast to clients.", @event.Id);
    }

    protected async Task ProcessManyAsync(IEnumerable<Event> events, CancellationToken cancellationToken)
    {
        var eventList = events as IReadOnlyList<Event> ?? events.ToList();
        await repository.AddManyAsync(eventList, cancellationToken);
        Logger.LogInformation("[DB] Batch of {Count} events saved.", eventList.Count);
        await Task.WhenAll(eventList.Select(e => notificationService.BroadcastEventAsync(e, cancellationToken)));
        Logger.LogInformation("[SignalR] Batch of {Count} events broadcast to clients.", eventList.Count);
    }
}
