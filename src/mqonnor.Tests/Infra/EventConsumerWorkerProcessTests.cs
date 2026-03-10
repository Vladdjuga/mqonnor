using System.Threading.Channels;
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
            _notificationService,
            Channel.CreateUnbounded<IReadOnlyList<Event>>());
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
    public async Task ProcessManyAsync_EnqueuesBatchToDbChannel()
    {
        var dbChannel = Channel.CreateUnbounded<IReadOnlyList<Event>>();
        var worker = new TestWorker(
            Substitute.For<IEventBus>(),
            NullLogger<EventConsumerWorker>.Instance,
            _repository,
            _notificationService,
            dbChannel);
        var events = Enumerable.Range(0, 3).Select(_ => MakeEvent()).ToList();

        await worker.ExecProcessManyAsync(events);

        dbChannel.Reader.TryRead(out var enqueuedBatch);
        Assert.NotNull(enqueuedBatch);
        Assert.True(enqueuedBatch.SequenceEqual(events));
    }

    [Fact]
    public async Task ProcessManyAsync_BroadcastsViaBroadcastMany()
    {
        var events = Enumerable.Range(0, 3).Select(_ => MakeEvent()).ToList();

        await _worker.ExecProcessManyAsync(events);

        await _notificationService.Received(1).BroadcastManyAsync(
            Arg.Is<IReadOnlyList<Event>>(e => e.SequenceEqual(events)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessManyAsync_EmptyCollection_NoBroadcast()
    {
        await _worker.ExecProcessManyAsync([]);

        await _notificationService.DidNotReceive().BroadcastManyAsync(Arg.Any<IReadOnlyList<Event>>(), Arg.Any<CancellationToken>());
    }

    private sealed class TestWorker(
        IEventBus eventBus,
        ILogger<EventConsumerWorker> logger,
        IEventRepository repository,
        INotificationService notificationService,
        Channel<IReadOnlyList<Event>> dbChannel)
        : EventConsumerWorker(eventBus, logger, repository, notificationService, dbChannel)
    {
        protected override Task RunLoopAsync(CancellationToken stoppingToken) => Task.CompletedTask;

        public Task ExecProcessAsync(Event @event, CancellationToken ct = default) =>
            ProcessAsync(@event, ct);

        public Task ExecProcessManyAsync(IEnumerable<Event> events, CancellationToken ct = default) =>
            ProcessManyAsync(events, ct);
    }
}
