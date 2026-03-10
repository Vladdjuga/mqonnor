using Microsoft.Extensions.Configuration;
using mqonnor.Application.Messaging;
using mqonnor.Infra.Workers;

namespace mqonnor.API.DI;

public static class WorkerExtensions
{
    public static IServiceCollection AddWorkers(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EventConsumerOptions>(configuration.GetSection(EventConsumerOptions.SectionName));

        var options = configuration
            .GetSection(EventConsumerOptions.SectionName)
            .Get<EventConsumerOptions>() ?? new EventConsumerOptions();

        var count = Math.Max(1, options.WorkerCount);

        for (var i = 0; i < count; i++)
        {
            _ = options.Mode switch
            {
                EventConsumerMode.One   => services.AddHostedService<OneByOneEventConsumerWorker>(),
                EventConsumerMode.Batch => services.AddHostedService<BatchEventConsumerWorker>(),
                _                       => services.AddHostedService<AllEventConsumerWorker>(),
            };
        }

        return services;
    }
}
