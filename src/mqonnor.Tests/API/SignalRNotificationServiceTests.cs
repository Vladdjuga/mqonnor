using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using mqonnor.API.DTOs;
using mqonnor.API.Hubs;
using mqonnor.API.Mappers;
using mqonnor.API.Services;
using mqonnor.Application.Mappers;
using mqonnor.Domain.Entities;
using NSubstitute;

namespace mqonnor.Tests.API;

public class SignalRNotificationServiceTests
{
    private readonly IHubContext<EventHub, IEventHubClient> _hubContext =
        Substitute.For<IHubContext<EventHub, IEventHubClient>>();
    private readonly IHubClients<IEventHubClient> _hubClients =
        Substitute.For<IHubClients<IEventHubClient>>();
    private readonly IEventHubClient _allClients =
        Substitute.For<IEventHubClient>();
    private readonly IMapper<Event, BroadcastEventDto> _mapper =
        Substitute.For<IMapper<Event, BroadcastEventDto>>();
    private readonly SignalRNotificationService _service;

    private static readonly BroadcastEventDto DummyDto = new(
        Guid.NewGuid(),
        JsonDocument.Parse("{}").RootElement,
        "UTF-8",
        "test");

    public SignalRNotificationServiceTests()
    {
        _hubContext.Clients.Returns(_hubClients);
        _hubClients.All.Returns(_allClients);
        _allClients.Notify(Arg.Any<BroadcastEventDto>()).Returns(Task.CompletedTask);
        _allClients.NotifyBatch(Arg.Any<IReadOnlyList<BroadcastEventDto>>()).Returns(Task.CompletedTask);
        _mapper.Map(Arg.Any<Event>()).Returns(DummyDto);
        _service = new SignalRNotificationService(_hubContext, _mapper);
    }

    private static Event MakeEvent() => new(
        Guid.NewGuid(), [1, 2, 3], new EventMetainfo("UTF-8", 3, "test-source"));

    [Fact]
    public async Task BroadcastEventAsync_MapsEvent()
    {
        var @event = MakeEvent();

        await _service.BroadcastEventAsync(@event);

        _mapper.Received(1).Map(@event);
    }

    [Fact]
    public async Task BroadcastEventAsync_CallsClientsAllNotifyWithMappedDto()
    {
        var @event = MakeEvent();

        await _service.BroadcastEventAsync(@event);

        await _allClients.Received(1).Notify(DummyDto);
    }

    [Fact]
    public async Task BroadcastManyAsync_MapsAllEvents()
    {
        var events = new[] { MakeEvent(), MakeEvent(), MakeEvent() };

        await _service.BroadcastManyAsync(events);

        _mapper.Received(3).Map(Arg.Any<Event>());
    }

    [Fact]
    public async Task BroadcastManyAsync_CallsNotifyBatchOnce()
    {
        var events = new[] { MakeEvent(), MakeEvent() };

        await _service.BroadcastManyAsync(events);

        await _allClients.Received(1).NotifyBatch(Arg.Any<IReadOnlyList<BroadcastEventDto>>());
    }
}
