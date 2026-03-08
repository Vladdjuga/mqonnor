using System.Runtime.CompilerServices;
using System.Threading.Channels;
using mqonnor.Application.Messaging;
using mqonnor.Domain.Entities;

namespace mqonnor.Infra.Messaging;

public sealed class ChannelEventBus(Channel<Event> channel) : IEventBus
{
    public ValueTask PublishAsync(Event @event, CancellationToken cancellationToken = default) =>
        channel.Writer.WriteAsync(@event, cancellationToken);

    public ValueTask<Event> ConsumeAsync(CancellationToken cancellationToken = default) =>
        channel.Reader.ReadAsync(cancellationToken);

    public IAsyncEnumerable<Event> ConsumeAllAsync(CancellationToken cancellationToken = default) =>
        channel.Reader.ReadAllAsync(cancellationToken);
}
