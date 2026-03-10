using mqonnor.Domain.Entities;

namespace mqonnor.Application.Services;

public interface INotificationService
{
    Task BroadcastEventAsync(Event @event, CancellationToken cancellationToken = default);
}
