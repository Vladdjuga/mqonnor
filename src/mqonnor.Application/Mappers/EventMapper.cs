using mqonnor.Application.DTOs;
using mqonnor.Domain.Entities;

namespace mqonnor.Application.Mappers;

public sealed class EventMapper : IMapper<PublishEventDto, Event>
{
    public Event Map(PublishEventDto source) => new(
        Guid.NewGuid(),
        source.Payload,
        new EventMetainfo(source.Encoding, source.Payload.Length, source.Source)
    );
}
