using mqonnor.Application.Abstractions;

namespace mqonnor.Infra.Mediator;

public class ConcreteMediator (IServiceProvider serviceProvider) :IMediator
{
    public async Task NotifyAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand
    {
        if (serviceProvider.GetService(typeof(ICommandHandler<TCommand>)) is ICommandHandler<TCommand> handler)
            await handler.HandleAsync(command, cancellationToken);
    }
}