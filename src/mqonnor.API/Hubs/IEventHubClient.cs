using mqonnor.API.DTOs;

namespace mqonnor.API.Hubs;

public interface IEventHubClient
{
    Task Error(string message);
    Task Notify(BroadcastEventDto dto);
}
