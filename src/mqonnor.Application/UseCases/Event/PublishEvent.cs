using mqonnor.Application.Abstractions;
using mqonnor.Application.DTOs;
using mqonnor.Application.Mappers;
using mqonnor.Application.Messaging;
using mqonnor.Domain.Primitives;

namespace mqonnor.Application.UseCases.Event;

public sealed record PublishEventCommand(PublishEventDto Dto) : ICommand;

public sealed class PublishEventCommandHandler(
    IMapper<PublishEventDto, Domain.Entities.Event> mapper,
    IEventBus eventBus) : ICommandHandler<PublishEventCommand, Result>
{
    public async Task<Result> HandleAsync(PublishEventCommand command, CancellationToken cancellationToken = default)
    {
        var @event = mapper.Map(command.Dto);
        await eventBus.PublishAsync(@event, cancellationToken);
        return Result.Success();
    }
}
