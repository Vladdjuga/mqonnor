using System.Text.Json;
using mqonnor.Application.Mappers;

namespace mqonnor.API.Mappers;

public sealed class JsonPayloadMapper : IMapper<byte[], JsonElement>
{
    public JsonElement Map(byte[] source)
        => JsonDocument.Parse(source).RootElement;
}
