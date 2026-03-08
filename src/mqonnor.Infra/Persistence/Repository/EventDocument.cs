using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace mqonnor.Infra.Persistence.Repository;

internal sealed class EventDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; init; }

    [BsonElement("payload")]
    public byte[] Payload { get; init; } = [];

    [BsonElement("encoding")]
    public string Encoding { get; init; } = string.Empty;

    [BsonElement("length")]
    public int Length { get; init; }

    [BsonElement("source")]
    public string Source { get; init; } = string.Empty;
}
