using mqonnor.API.DTOs;

namespace mqonnor.API.Hubs;

public interface IEventHubClient
{
    Task Error(string message);
    Task Notify(BroadcastEventDto dto);
    Task NotifyBatch(IReadOnlyList<BroadcastEventDto> events);
}
