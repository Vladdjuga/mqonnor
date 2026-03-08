using mqonnor.Application.Abstractions;
using mqonnor.Application.Exceptions;
using mqonnor.Domain.Primitives;

namespace mqonnor.Infra.Mediator;

public class ConcreteMediator (IServiceProvider serviceProvider) :IMediator
{
    public async Task<TResult> NotifyAsync<TCommand,TResult>(TCommand command, CancellationToken cancellationToken = default) 
        where TCommand : ICommand
        where TResult : Result
    {
        if (serviceProvider.GetService(typeof(ICommandHandler<TCommand, Result>)) is not
            ICommandHandler<TCommand, Result> handler)
            throw new NoSuchCommandHandlerException(
                $"No such handler for command type : {typeof(TCommand).Name}"); // DI problems
        if (await handler.HandleAsync(command, cancellationToken) is TResult res)
            return res;
        throw new CastToResultFailedException("Cast from Result to TResult failed."); // This shouldn`t happen
    }
}