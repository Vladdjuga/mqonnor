using System.Text.Json;
using mqonnor.API.DTOs;
using mqonnor.API.Mappers;
using mqonnor.Application.DTOs;
using mqonnor.Application.Mappers;
using mqonnor.Domain.Entities;

namespace mqonnor.API.DI;

public static class MapperExtensions
{
    public static IServiceCollection AddMappers(this IServiceCollection services)
    {
        services.AddSingleton<IMapper<PublishEventDto, Event>, EventMapper>();
        services.AddSingleton<IMapper<byte[], JsonElement>, JsonPayloadMapper>();
        services.AddSingleton<IMapper<Event, BroadcastEventDto>, EventBroadcastMapper>();

        return services;
    }
}
