namespace mqonnor.Application.Abstractions;

public interface IMediator
{
    Task NotifyAsync(ICommand command, CancellationToken cancellationToken = default);
}
