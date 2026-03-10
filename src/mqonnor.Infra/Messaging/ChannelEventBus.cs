using System.Threading.Channels;
using mqonnor.Application.Messaging;
using mqonnor.Domain.Entities;
using mqonnor.Infra.Extensions;

namespace mqonnor.Infra.Messaging;

public sealed class ChannelEventBus(Channel<Event> channel) : IEventBus
{
    public ValueTask PublishAsync(Event @event, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return channel.Writer.WriteAsync(@event, cancellationToken);
    }

    public ValueTask<Event> ConsumeAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return channel.Reader.ReadAsync(cancellationToken);
    }

    public IAsyncEnumerable<Event> ConsumeAllAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return channel.Reader.ReadAllAsync(cancellationToken);
    }

    public async ValueTask<IEnumerable<Event>> ConsumeBatchAsync(int batch = 32, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var list = new List<Event>(batch); // capacity is the batch size, dont add more
        var count = await channel.Reader.ReadBatchAsync(list, batch, cancellationToken);
        return count <= 0 ? [] : list;
    }
}
