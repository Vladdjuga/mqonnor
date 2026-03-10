using mqonnor.Application.Services;
using mqonnor.API.Services;

namespace mqonnor.API.DI;

public static class NotificationExtensions
{
    public static IServiceCollection AddNotifications(this IServiceCollection services)
    {
        services.AddSingleton<INotificationService, SignalRNotificationService>();
        return services;
    }
}
