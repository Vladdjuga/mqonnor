using System.Threading.Channels;
using mqonnor.Application.Messaging;
using mqonnor.Domain.Entities;
using mqonnor.Infra;
using mqonnor.Infra.Workers;

namespace mqonnor.API.DI;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructurePersistence(configuration);

        var capacity = configuration
            .GetSection(EventConsumerOptions.SectionName)
            .Get<EventConsumerOptions>()?.ChannelCapacity ?? 65536;

        services.AddInfrastructureMessaging(capacity);

        // Write-behind channel for DB persistence — decouples MongoDB from the broadcast hot path.
        // Capacity = 1024 batches × 512 events = ~500k events buffered.
        services.AddSingleton(_ => Channel.CreateBounded<IReadOnlyList<Event>>(
            new BoundedChannelOptions(1024)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            }));
        services.AddHostedService<DbPersistenceWorker>();

        return services;
    }
}
