using mqonnor.Domain.Entities;

namespace mqonnor.Application.Persistence;

public interface IEventPersistence
{
    Task SaveAsync(Event @event, CancellationToken cancellationToken = default);
    Task<Event?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Event>> FindAllAsync(CancellationToken cancellationToken = default);
    Task RemoveAsync(Guid id, CancellationToken cancellationToken = default);
}
