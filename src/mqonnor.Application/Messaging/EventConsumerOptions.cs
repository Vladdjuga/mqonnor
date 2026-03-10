namespace mqonnor.Application.Messaging;

public sealed class EventConsumerOptions
{
    public const string SectionName = "EventConsumer";

    public EventConsumerMode Mode { get; init; } = EventConsumerMode.All;

    /// <summary>Maximum number of events per iteration. Only used when <see cref="Mode"/> is <see cref="EventConsumerMode.Batch"/>.</summary>
    public int BatchSize { get; init; } = 256;

    /// <summary>Bounded capacity of the in-process Channel&lt;Event&gt;. Higher values reduce backpressure under burst load.</summary>
    public int ChannelCapacity { get; init; } = 65536;

    /// <summary>Number of concurrent consumer worker instances. Each worker drains the shared channel independently.</summary>
    public int WorkerCount { get; init; } = 1;

    /// <summary>Number of parallel DB persistence workers. Each one independently drains the write-behind channel into MongoDB.</summary>
    public int DbWorkerCount { get; init; } = 2;
}
