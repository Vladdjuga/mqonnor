using mqonnor.Domain.Entities;

namespace mqonnor.Application.Messaging;

public interface IEventBus
{
    /// <summary>
    /// Publishes an event into the next available slot of the ring buffer.
    /// </summary>
    void Publish(Event @event);

    /// <summary>
    /// Attempts to consume the next single event from the ring buffer.
    /// Returns false if no events are available.
    /// </summary>
    bool TryConsume(out Event? @event);

    /// <summary>
    /// Attempts to consume a batch of available events as a zero-copy span
    /// over the ring buffer's backing Event[] store.
    /// Returns an empty span if no events are available.
    /// </summary>
    ReadOnlySpan<Event> TryConsumeBatch();
}
