using mqonnor.Application.DTOs;

namespace mqonnor.API.Hubs;

public interface IEventHubClient
{
    Task Error(string message);
    Task Notify(PublishEventDto dto);
}
