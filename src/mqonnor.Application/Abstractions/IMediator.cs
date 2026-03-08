using mqonnor.Domain.Primitives;

namespace mqonnor.Application.Abstractions;

public interface IMediator
{
    Task<TResult> NotifyAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : ICommand
        where TResult : Result;
}
