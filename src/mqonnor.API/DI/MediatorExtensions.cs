using mqonnor.Application.Abstractions;
using mqonnor.Application.UseCases.Event;
using mqonnor.Infra.Mediator;

namespace mqonnor.API.DI;

public static class MediatorExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddMediator()
        {
            services.AddScoped<IMediator, ConcreteMediator>();
            return services;
        }

        public IServiceCollection AddCommandHandlers()
        {
            services.AddScoped<ICommandHandler<PublishEventCommand>, PublishEventCommandHandler>();

            return services;
        }
    }
}
