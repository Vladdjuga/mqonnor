using mqonnor.Application.DTOs;
using mqonnor.Application.Mappers;
using mqonnor.Domain.Entities;

namespace mqonnor.API.DI;

public static class MapperExtensions
{
    public static IServiceCollection AddMappers(this IServiceCollection services)
    {
        services.AddScoped<IMapper<PublishEventDto, Event>, EventMapper>();

        return services;
    }
}
