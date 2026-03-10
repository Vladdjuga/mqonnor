namespace mqonnor.Application.Messaging;

public enum EventConsumerMode
{
    /// <summary>Consume one event at a time via <see cref="IEventBus.ConsumeAsync"/>.</summary>
    One,

    /// <summary>Consume a bounded batch of events at a time via <see cref="IEventBus.ConsumeBatchAsync"/>.</summary>
    Batch,

    /// <summary>Consume all events as a continuous async stream via <see cref="IEventBus.ConsumeAllAsync"/>.</summary>
    All
}
