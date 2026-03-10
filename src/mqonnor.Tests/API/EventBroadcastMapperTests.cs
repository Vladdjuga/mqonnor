using System.Text.Json;
using mqonnor.API.DTOs;
using mqonnor.API.Mappers;
using mqonnor.Application.Mappers;
using mqonnor.Domain.Entities;
using NSubstitute;

namespace mqonnor.Tests.API;

public class EventBroadcastMapperTests
{
    private readonly IMapper<byte[], JsonElement> _jsonMapper = Substitute.For<IMapper<byte[], JsonElement>>();
    private readonly EventBroadcastMapper _mapper;

    private static readonly JsonElement DummyJson =
        JsonDocument.Parse("""{"x":1}""").RootElement;

    public EventBroadcastMapperTests()
    {
        _jsonMapper.Map(Arg.Any<byte[]>()).Returns(DummyJson);
        _mapper = new EventBroadcastMapper(_jsonMapper);
    }

    private static Event MakeEvent(byte[]? payload = null) => new(
        Guid.NewGuid(),
        payload ?? [1, 2, 3],
        new EventMetainfo("UTF-8", 3, "test-source"));

    [Fact]
    public void Map_PreservesId()
    {
        var @event = MakeEvent();

        var dto = _mapper.Map(@event);

        Assert.Equal(@event.Id, dto.Id);
    }

    [Fact]
    public void Map_PreservesEncoding()
    {
        var @event = MakeEvent();

        var dto = _mapper.Map(@event);

        Assert.Equal(@event.Metainfo.Encoding, dto.Encoding);
    }

    [Fact]
    public void Map_PreservesSource()
    {
        var @event = MakeEvent();

        var dto = _mapper.Map(@event);

        Assert.Equal(@event.Metainfo.Source, dto.Source);
    }

    [Fact]
    public void Map_CallsJsonMapperWithPayload()
    {
        var payload = new byte[] { 10, 20, 30 };
        var @event = MakeEvent(payload);

        _mapper.Map(@event);

        _jsonMapper.Received(1).Map(payload);
    }

    [Fact]
    public void Map_PayloadIsReturnedFromJsonMapper()
    {
        var @event = MakeEvent();

        var dto = _mapper.Map(@event);

        Assert.Equal(DummyJson, dto.Payload);
    }
}
