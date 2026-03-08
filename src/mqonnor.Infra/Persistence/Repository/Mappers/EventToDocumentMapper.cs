using mqonnor.Application.Mappers;
using mqonnor.Domain.Entities;

namespace mqonnor.Infra.Persistence.Repository.Mappers;

internal sealed class EventToDocumentMapper : IMapper<Event, EventDocument>
{
    public EventDocument Map(Event source) => new()
    {
        Id = source.Id,
        Payload = source.Payload,
        Encoding = source.Metainfo.Encoding,
        Length = source.Metainfo.Length,
        Source = source.Metainfo.Source
    };
}
