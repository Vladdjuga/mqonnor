using mqonnor.Application.Messaging;
using mqonnor.Infra;

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

        return services;
    }
}
