using mqonnor.Application.Mappers;
using mqonnor.Domain.Entities;

namespace mqonnor.Infra.Persistence.Repository.Mappers;

internal sealed class EventDocumentToDomainMapper : IMapper<EventDocument, Event>
{
    public Event Map(EventDocument source) => new(
        source.Id,
        source.Payload,
        new EventMetainfo(source.Encoding, source.Length, source.Source)
    );
}
