using mqonnor.Domain.Entities;

namespace mqonnor.Application.Messaging;

public interface IEventBus
{
    /// <summary>
    /// Publishes an event into the channel.
    /// Awaits if the bounded channel is full, providing natural backpressure.
    /// </summary>
    ValueTask PublishAsync(Event @event, CancellationToken cancellationToken = default);

    /// <summary>
    /// Awaits the next available event from the channel.
    /// Workers will suspend here until a new event is written.
    /// </summary>
    ValueTask<Event> ConsumeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns an async stream of all events as they arrive.
    /// Intended for long-running workers that process events continuously.
    /// </summary>
    IAsyncEnumerable<Event> ConsumeAllAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Returns an async stream of all events as they arrive.
    /// Intended for long-running workers that process events continuously.
    /// </summary>
    ValueTask<IEnumerable<Event>> ConsumeBatchAsync(int batch = 32, CancellationToken cancellationToken = default);
}
