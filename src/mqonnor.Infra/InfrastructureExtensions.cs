using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using mqonnor.Application.Mappers;
using mqonnor.Domain.Entities;
using mqonnor.Domain.Repositories;
using mqonnor.Infra.Persistence.Repository;
using mqonnor.Infra.Persistence.Repository.Mappers;

namespace mqonnor.Infra;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructurePersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("MongoDB")
            ?? throw new InvalidOperationException("MongoDB connection string is not configured.");

        var databaseName = configuration["MongoDB:Database"]
            ?? throw new InvalidOperationException("MongoDB database name is not configured.");

        services.AddSingleton<IMongoClient>(_ => new MongoClient(connectionString));

        services.AddScoped(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var database = client.GetDatabase(databaseName);
            return database.GetCollection<EventDocument>("events");
        });

        services.AddScoped<IEventRepository, EventRepository>();

        services.AddScoped<IMapper<Event, EventDocument>, EventToDocumentMapper>();
        services.AddScoped<IMapper<EventDocument, Event>, EventDocumentToDomainMapper>();

        return services;
    }
}
