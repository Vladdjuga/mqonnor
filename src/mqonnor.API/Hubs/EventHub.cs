using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using mqonnor.Application.Abstractions;
using mqonnor.Application.DTOs;
using mqonnor.Application.UseCases.Event;
using mqonnor.Domain.Primitives;

namespace mqonnor.API.Hubs;

public sealed class EventHub(IMediator mediator, ILogger<EventHub> logger) : Hub<IEventHubClient>
{
    public override Task OnConnectedAsync()
    {
        logger.LogInformation("[SignalR] Client connected: {ConnectionId}", Context.ConnectionId);
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception is null)
            logger.LogInformation("[SignalR] Client disconnected: {ConnectionId}", Context.ConnectionId);
        else
            logger.LogWarning(exception, "[SignalR] Client disconnected with error: {ConnectionId}", Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    public async Task Publish(PublishEventDto dto)
    {
        logger.LogInformation("[Hub] Publish from {ConnectionId} — source={Source} encoding={Encoding} bytes={Bytes}",
            Context.ConnectionId, dto.Source, dto.Encoding, dto.Payload.Length);

        var result = await mediator.NotifyAsync<PublishEventCommand, Result>(
            new PublishEventCommand(dto),
            Context.ConnectionAborted);

        if (result.IsFailure)
        {
            logger.LogWarning("[Hub] Publish failed for {ConnectionId}: {Error}", Context.ConnectionId, result.Error);
            await Clients.Caller.Error(result.Error!);
        }
    }
}
