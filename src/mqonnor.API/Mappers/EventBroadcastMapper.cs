using System.Text.Json;
using mqonnor.API.DTOs;
using mqonnor.Application.Mappers;
using mqonnor.Domain.Entities;

namespace mqonnor.API.Mappers;

public sealed class EventBroadcastMapper(IMapper<byte[], JsonElement> jsonMapper) : IMapper<Event, BroadcastEventDto>
{
    public BroadcastEventDto Map(Event source) => new(
        source.Id,
        jsonMapper.Map(source.Payload),
        source.Metainfo.Encoding,
        source.Metainfo.Source
    );
}
