namespace mqonnor.API.DI;

public static class SignalRExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSignalRHub()
        {
            services.AddSignalR();
            return services;
        }
    }
}
