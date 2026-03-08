using mqonnor.Application.Abstractions;

namespace mqonnor.API.DI;

public static class MediatorExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMediator()
        {
            // TODO: register IMediator implementation
            // services.AddScoped<IMediator, Mediator>();

            return services;
        }

        public IServiceCollection AddCommandHandlers()
        {
            // TODO: register command handlers explicitly as scoped
            // services.AddScoped<ICommandHandler<PublishEventCommand>, PublishEventCommandHandler>();

            return services;
        }
    }
}
