using mqonnor.Application.Abstractions;

namespace mqonnor.API.DI;

public static class MediatorExtensions
{
    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        // TODO: register IMediator implementation
        // services.AddScoped<IMediator, Mediator>();

        return services;
    }

    public static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        // TODO: register command handlers explicitly as scoped
        // services.AddScoped<ICommandHandler<PublishEventCommand>, PublishEventCommandHandler>();

        return services;
    }
}
