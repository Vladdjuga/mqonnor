using Microsoft.Extensions.Configuration;
using mqonnor.Application.Messaging;
using mqonnor.Infra.Workers;

namespace mqonnor.API.DI;

public static class WorkerExtensions
{
    public static IServiceCollection AddWorkers(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EventConsumerOptions>(configuration.GetSection(EventConsumerOptions.SectionName));

        var mode = configuration
            .GetSection(EventConsumerOptions.SectionName)
            .Get<EventConsumerOptions>()?.Mode ?? EventConsumerMode.All;

        return mode switch
        {
            EventConsumerMode.One   => services.AddHostedService<OneByOneEventConsumerWorker>(),
            EventConsumerMode.Batch => services.AddHostedService<BatchEventConsumerWorker>(),
            _                       => services.AddHostedService<AllEventConsumerWorker>(),
        };
    }
}
