using System.Text.Json;
using mqonnor.API.Mappers;

namespace mqonnor.Tests.API;

public class JsonPayloadMapperTests
{
    private readonly JsonPayloadMapper _mapper = new();

    [Fact]
    public void Map_ValidJsonObject_ReturnsObjectElement()
    {
        var bytes = """{"key":"value"}"""u8.ToArray();

        var result = _mapper.Map(bytes);

        Assert.Equal(JsonValueKind.Object, result.ValueKind);
        Assert.Equal("value", result.GetProperty("key").GetString());
    }

    [Fact]
    public void Map_ValidJsonArray_ReturnsArrayElement()
    {
        var bytes = "[1,2,3]"u8.ToArray();

        var result = _mapper.Map(bytes);

        Assert.Equal(JsonValueKind.Array, result.ValueKind);
        Assert.Equal(3, result.GetArrayLength());
    }

    [Fact]
    public void Map_InvalidJsonBytes_ThrowsJsonException()
    {
        var bytes = "not json"u8.ToArray();

        Assert.ThrowsAny<JsonException>(() => _mapper.Map(bytes));
    }
}
