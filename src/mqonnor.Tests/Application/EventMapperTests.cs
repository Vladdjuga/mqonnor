using mqonnor.Application.DTOs;
using mqonnor.Application.Mappers;
using mqonnor.Domain.Entities;

namespace mqonnor.Tests.Application;

public class EventMapperTests
{
    private readonly EventMapper _sut = new();

    [Fact]
    public void Map_Payload_IsPreserved()
    {
        var payload = new byte[] { 1, 2, 3 };
        var dto = new PublishEventDto(payload, "UTF-8", "test-service");

        var result = _sut.Map(dto);

        Assert.Equal(payload, result.Payload);
    }

    [Fact]
    public void Map_Metainfo_EncodingAndSourceArePreserved()
    {
        var dto = new PublishEventDto([10, 20], "ASCII", "origin");

        var result = _sut.Map(dto);

        Assert.Equal("ASCII", result.Metainfo.Encoding);
        Assert.Equal("origin", result.Metainfo.Source);
    }

    [Fact]
    public void Map_Metainfo_LengthDerivedFromPayload()
    {
        var payload = new byte[7];
        var dto = new PublishEventDto(payload, "UTF-8", "src");

        var result = _sut.Map(dto);

        Assert.Equal(7, result.Metainfo.Length);
    }

    [Fact]
    public void Map_Id_IsNewGuid()
    {
        var dto = new PublishEventDto([1], "UTF-8", "src");

        var a = _sut.Map(dto);
        var b = _sut.Map(dto);

        Assert.NotEqual(Guid.Empty, a.Id);
        Assert.NotEqual(a.Id, b.Id);
    }
}
