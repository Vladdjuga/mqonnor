using mqonnor.Infra.Workers;

namespace mqonnor.API.DI;

public static class WorkerExtensions
{
    public static IServiceCollection AddWorkers(this IServiceCollection services)
    {
        services.AddHostedService<EventConsumerWorker>();

        return services;
    }
}
