using MongoDB.Driver;
using mqonnor.Application.Mappers;
using mqonnor.Domain.Entities;
using mqonnor.Domain.Repositories;
using mqonnor.Infra.Persistence.Repository.Mappers;

namespace mqonnor.Infra.Persistence.Repository;

internal sealed class EventRepository(
    IMongoCollection<EventDocument> collection,
    IMapper<Event, EventDocument> toDocument,
    IMapper<EventDocument, Event> toDomain) : IEventRepository
{
    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var doc = await collection
            .Find(e => e.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return doc is null ? null : toDomain.Map(doc);
    }

    public async Task<IEnumerable<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var docs = await collection
            .Find(Builders<EventDocument>.Filter.Empty)
            .ToListAsync(cancellationToken);

        return docs.Select(toDomain.Map);
    }

    public Task AddAsync(Event @event, CancellationToken cancellationToken = default) =>
        collection.InsertOneAsync(toDocument.Map(@event), cancellationToken: cancellationToken);

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) =>
        collection.DeleteOneAsync(e => e.Id == id, cancellationToken);
}
