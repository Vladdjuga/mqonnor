using mqonnor.Domain.Primitives;

namespace mqonnor.Application.Abstractions;

public interface ICommandHandler<in TCommand, TResult>
    where TCommand : ICommand
    where TResult : Result
{
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);
}
