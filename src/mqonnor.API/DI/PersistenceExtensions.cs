using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using mqonnor.Application.Messaging;
using mqonnor.Domain.Entities;
using mqonnor.Domain.Repositories;
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

        var consumerOptions = configuration
            .GetSection(EventConsumerOptions.SectionName)
            .Get<EventConsumerOptions>() ?? new EventConsumerOptions();

        var dbWorkerCount = Math.Max(1, consumerOptions.DbWorkerCount);

        // Write-behind channel for DB persistence — UNBOUNDED so broadcast workers are never
        // blocked waiting for MongoDB. The main event channel (ChannelCapacity) already provides
        // ingest flow control. Bounding this channel caused broadcast workers to stall when
        // MongoDB write throughput fell below the ingest rate, filling the main channel and
        // blocking producers — the root cause of tail latency spikes.
        services.AddSingleton(_ => Channel.CreateUnbounded<IReadOnlyList<Event>>(
            new UnboundedChannelOptions
            {
                SingleReader = dbWorkerCount == 1,
                SingleWriter = false
            }));

        for (var i = 0; i < dbWorkerCount; i++)
        {
            var id = i;
            services.AddHostedService(sp =>
                new DbPersistenceWorker(
                    sp.GetRequiredService<Channel<IReadOnlyList<Event>>>(),
                    sp.GetRequiredService<IEventRepository>(),
                    sp.GetRequiredService<ILogger<DbPersistenceWorker>>(),
                    id));
        }

        return services;
    }
}
