using Microsoft.AspNetCore.SignalR;
using mqonnor.Application.Abstractions;
using mqonnor.Application.DTOs;
using mqonnor.Application.UseCases.Event;
using mqonnor.Domain.Primitives;

namespace mqonnor.API.Hubs;

public sealed class EventHub(IMediator mediator) : Hub<IEventHubClient>
{
    public async Task Publish(PublishEventDto dto)
    {
        var result = await mediator.NotifyAsync<PublishEventCommand, Result>(
            new PublishEventCommand(dto),
            Context.ConnectionAborted);

        if (result.IsFailure)
        {
            await Clients.Caller.Error(result.Error!);
            return;
        }

        await Clients.All.Notify(dto);
    }
}
