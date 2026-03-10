using mqonnor.Infra;

namespace mqonnor.API.DI;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructurePersistence(configuration);
        services.AddInfrastructureMessaging();

        return services;
    }
}
