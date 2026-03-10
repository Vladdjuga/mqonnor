using System.Threading.Channels;
using mqonnor.Domain.Entities;
using mqonnor.Infra.Extensions;

namespace mqonnor.Tests.Infra;

public class ChannelExtensionsTests
{
    private static Event MakeEvent() =>
        new(Guid.NewGuid(), [1, 2, 3], new EventMetainfo("UTF-8", 3, "test"));

    private static Channel<Event> CreateChannel(int capacity = 64) =>
        Channel.CreateBounded<Event>(capacity);

    [Fact]
    public async Task ReadBatchAsync_ReadsExactlyMaxSizeWhenMoreAvailable()
    {
        var channel = CreateChannel();
        for (var i = 0; i < 10; i++)
            channel.Writer.TryWrite(MakeEvent());

        var batch = new List<Event>();
        var count = await channel.Reader.ReadBatchAsync(batch, maxSize: 5);

        Assert.Equal(5, count);
        Assert.Equal(5, batch.Count);
    }

    [Fact]
    public async Task ReadBatchAsync_ReadsFewerItemsWhenLessThanMaxSizeAvailable()
    {
        var channel = CreateChannel();
        for (var i = 0; i < 3; i++)
            channel.Writer.TryWrite(MakeEvent());

        var batch = new List<Event>();
        var count = await channel.Reader.ReadBatchAsync(batch, maxSize: 10);

        Assert.Equal(3, count);
        Assert.Equal(3, batch.Count);
    }

    [Fact]
    public async Task ReadBatchAsync_ReturnsZeroWhenChannelCompleted()
    {
        var channel = CreateChannel();
        channel.Writer.Complete();

        var batch = new List<Event>();
        var count = await channel.Reader.ReadBatchAsync(batch, maxSize: 10);

        Assert.Equal(0, count);
        Assert.Empty(batch);
    }

    [Fact]
    public async Task ReadBatchAsync_AppendsToExistingListContent()
    {
        var channel = CreateChannel();
        var existing = MakeEvent();
        channel.Writer.TryWrite(MakeEvent());

        var batch = new List<Event> { existing };
        var count = await channel.Reader.ReadBatchAsync(batch, maxSize: 5);

        Assert.Equal(1, count);
        Assert.Equal(2, batch.Count);
        Assert.Equal(existing, batch[0]);
    }

    [Fact]
    public async Task ReadBatchAsync_CancelledToken_Throws()
    {
        var channel = CreateChannel();
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        var batch = new List<Event>();
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => channel.Reader.ReadBatchAsync(batch, maxSize: 5, ct: cts.Token).AsTask());
    }
}
