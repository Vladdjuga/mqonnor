using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using mqonnor.Application.Messaging;
using mqonnor.Domain.Entities;
using mqonnor.Domain.Repositories;

namespace mqonnor.Infra.Workers;

public abstract class EventConsumerWorker(
    IEventBus eventBus,
    ILogger<EventConsumerWorker> logger,
    IEventRepository repository) : BackgroundService
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

    protected Task ProcessAsync(Event @event, CancellationToken cancellationToken)
        => repository.AddAsync(@event, cancellationToken);

    protected Task ProcessManyAsync(IEnumerable<Event> events, CancellationToken cancellationToken)
        => repository.AddManyAsync(events, cancellationToken);
}
