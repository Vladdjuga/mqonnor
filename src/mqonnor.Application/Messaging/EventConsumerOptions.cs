namespace mqonnor.Application.Messaging;

public sealed class EventConsumerOptions
{
    public const string SectionName = "EventConsumer";

    public EventConsumerMode Mode { get; init; } = EventConsumerMode.All;

    /// <summary>Maximum number of events per iteration. Only used when <see cref="Mode"/> is <see cref="EventConsumerMode.Batch"/>.</summary>
    public int BatchSize { get; init; } = 32;
}
