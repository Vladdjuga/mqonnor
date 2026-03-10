using Microsoft.AspNetCore.SignalR;
using mqonnor.API.DTOs;
using mqonnor.API.Hubs;
using mqonnor.Application.Mappers;
using mqonnor.Application.Services;
using mqonnor.Domain.Entities;

namespace mqonnor.API.Services;

public sealed class SignalRNotificationService(
    IHubContext<EventHub, IEventHubClient> hubContext,
    IMapper<Event, BroadcastEventDto> mapper) : INotificationService
{
    public Task BroadcastEventAsync(Event @event, CancellationToken cancellationToken = default)
        => hubContext.Clients.All.Notify(mapper.Map(@event));

    public Task BroadcastManyAsync(IReadOnlyList<Event> events, CancellationToken cancellationToken = default)
        => hubContext.Clients.All.NotifyBatch(events.Select(mapper.Map).ToList());
}
