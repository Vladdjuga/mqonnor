using System.Threading.Channels;
using mqonnor.Domain.Entities;
using mqonnor.Infra.Messaging;

namespace mqonnor.Tests.Infra;

public class ChannelEventBusTests
{
    private static Event MakeEvent() =>
        new(Guid.NewGuid(), [1, 2, 3], new EventMetainfo("UTF-8", 3, "test"));

    private static ChannelEventBus CreateBus(int capacity = 16)
    {
        var channel = Channel.CreateBounded<Event>(capacity);
        return new ChannelEventBus(channel);
    }

    [Fact]
    public async Task PublishAsync_ThenConsumeAsync_ReturnsPublishedEvent()
    {
        var bus = CreateBus();
        var @event = MakeEvent();

        await bus.PublishAsync(@event);
        var consumed = await bus.ConsumeAsync();

        Assert.Equal(@event.Id, consumed.Id);
    }

    [Fact]
    public async Task ConsumeAllAsync_ReceivesAllPublishedEvents()
    {
        var bus = CreateBus();
        var events = Enumerable.Range(0, 5).Select(_ => MakeEvent()).ToList();

        foreach (var e in events)
            await bus.PublishAsync(e);

        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var consumed = new List<Event>();

        try
        {
            await foreach (var e in bus.ConsumeAllAsync(cts.Token))
            {
                consumed.Add(e);
                if (consumed.Count == events.Count) cts.Cancel();
            }
        }
        catch (OperationCanceledException) { }

        Assert.Equal(events.Select(e => e.Id), consumed.Select(e => e.Id));
    }

    [Fact]
    public async Task PublishAsync_CancelledToken_Throws()
    {
        var bus = CreateBus();
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => bus.PublishAsync(MakeEvent(), cts.Token).AsTask());
    }

    [Fact]
    public async Task ConsumeAsync_CancelledToken_Throws()
    {
        var bus = CreateBus();
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => bus.ConsumeAsync(cts.Token).AsTask());
    }

    [Fact]
    public async Task PublishAsync_BoundedChannelFull_Suspends()
    {
        var bus = CreateBus(capacity: 1);
        await bus.PublishAsync(MakeEvent());

        var publishTask = bus.PublishAsync(MakeEvent()).AsTask();

        await Task.Delay(50);
        Assert.False(publishTask.IsCompleted);

        await bus.ConsumeAsync();
        await publishTask;

        Assert.True(publishTask.IsCompleted);
    }

    [Fact]
    public async Task ConsumeBatchAsync_ReturnsUpToBatchSizeItems()
    {
        var bus = CreateBus();
        var events = Enumerable.Range(0, 10).Select(_ => MakeEvent()).ToList();
        foreach (var e in events)
            await bus.PublishAsync(e);

        var batch = (await bus.ConsumeBatchAsync(batch: 5)).ToList();

        Assert.Equal(5, batch.Count);
    }

    [Fact]
    public async Task ConsumeBatchAsync_ReturnsFewerItemsWhenLessThanBatchAvailable()
    {
        var bus = CreateBus();
        var events = Enumerable.Range(0, 3).Select(_ => MakeEvent()).ToList();
        foreach (var e in events)
            await bus.PublishAsync(e);

        var batch = (await bus.ConsumeBatchAsync(batch: 10)).ToList();

        Assert.Equal(3, batch.Count);
    }

    [Fact]
    public async Task ConsumeBatchAsync_CancelledToken_Throws()
    {
        var bus = CreateBus();
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => bus.ConsumeBatchAsync(cancellationToken: cts.Token).AsTask());
    }
}
