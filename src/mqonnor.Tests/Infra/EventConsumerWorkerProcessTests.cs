using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using mqonnor.Application.Messaging;
using mqonnor.Application.Services;
using mqonnor.Domain.Entities;
using mqonnor.Domain.Repositories;
using mqonnor.Infra.Workers;
using NSubstitute;

namespace mqonnor.Tests.Infra;

public class EventConsumerWorkerProcessTests
{
    private readonly IEventRepository _repository = Substitute.For<IEventRepository>();
    private readonly INotificationService _notificationService = Substitute.For<INotificationService>();
    private readonly TestWorker _worker;

    public EventConsumerWorkerProcessTests()
    {
        _worker = new TestWorker(
            Substitute.For<IEventBus>(),
            NullLogger<EventConsumerWorker>.Instance,
            _repository,
            _notificationService);
    }

    private static Event MakeEvent() =>
        new(Guid.NewGuid(), [1, 2, 3], new EventMetainfo("UTF-8", 3, "test"));

    [Fact]
    public async Task ProcessAsync_PersistsEvent()
    {
        var @event = MakeEvent();

        await _worker.ExecProcessAsync(@event);

        await _repository.Received(1).AddAsync(@event, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessAsync_BroadcastsEvent()
    {
        var @event = MakeEvent();

        await _worker.ExecProcessAsync(@event);

        await _notificationService.Received(1).BroadcastEventAsync(@event, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessManyAsync_PersistsBatch()
    {
        var events = Enumerable.Range(0, 3).Select(_ => MakeEvent()).ToList();

        await _worker.ExecProcessManyAsync(events);

        await _repository.Received(1).AddManyAsync(
            Arg.Is<IEnumerable<Event>>(e => e.SequenceEqual(events)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessManyAsync_BroadcastsEachEvent()
    {
        var events = Enumerable.Range(0, 3).Select(_ => MakeEvent()).ToList();

        await _worker.ExecProcessManyAsync(events);

        foreach (var @event in events)
            await _notificationService.Received(1).BroadcastEventAsync(@event, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessManyAsync_EmptyCollection_NoBroadcast()
    {
        await _worker.ExecProcessManyAsync([]);

        await _notificationService.DidNotReceive().BroadcastEventAsync(Arg.Any<Event>(), Arg.Any<CancellationToken>());
    }

    private sealed class TestWorker(
        IEventBus eventBus,
        ILogger<EventConsumerWorker> logger,
        IEventRepository repository,
        INotificationService notificationService)
        : EventConsumerWorker(eventBus, logger, repository, notificationService)
    {
        protected override Task RunLoopAsync(CancellationToken stoppingToken) => Task.CompletedTask;

        public Task ExecProcessAsync(Event @event, CancellationToken ct = default) =>
            ProcessAsync(@event, ct);

        public Task ExecProcessManyAsync(IEnumerable<Event> events, CancellationToken ct = default) =>
            ProcessManyAsync(events, ct);
    }
}
