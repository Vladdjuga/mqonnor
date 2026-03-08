namespace mqonnor.Domain;

public class Event
{
    public Guid Id { get; init; }
    public byte[] Payload { get; init; }
    public EventMetainfo Metainfo { get; init; }

    public Event(Guid id, byte[] payload, EventMetainfo metainfo)
    {
        Id = id;
        Payload = payload;
        Metainfo = metainfo;
    }
}
