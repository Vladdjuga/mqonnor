using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using mqonnor.Application.Messaging;
using mqonnor.Domain.Entities;
using mqonnor.Infra.Messaging;

namespace mqonnor.Infra;

public static class MessagingExtensions
{
    public static IServiceCollection AddInfrastructureMessaging(
        this IServiceCollection services,
        int channelCapacity = 4096)
    {
        services.AddSingleton(_ => Channel.CreateBounded<Event>(
            new BoundedChannelOptions(channelCapacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = false,
                SingleWriter = false
            }));

        services.AddSingleton<IEventBus, ChannelEventBus>();

        return services;
    }
}
