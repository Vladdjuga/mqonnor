namespace mqonnor.Application.Abstractions;

public interface IMediator
{
    Task NotifyAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;
}
