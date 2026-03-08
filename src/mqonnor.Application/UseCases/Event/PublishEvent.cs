using mqonnor.Application.Abstractions;
using mqonnor.Application.DTOs;
using mqonnor.Application.Mappers;
using mqonnor.Application.Messaging;

namespace mqonnor.Application.UseCases.Event;

public sealed record PublishEventCommand(PublishEventDto Dto) : ICommand;

public sealed class PublishEventCommandHandler(
    IMapper<PublishEventDto, Domain.Entities.Event> mapper,
    IEventBus eventBus) : ICommandHandler<PublishEventCommand>
{
    public ValueTask HandleAsync(PublishEventCommand command, CancellationToken cancellationToken = default)
    {
        var @event = mapper.Map(command.Dto);
        eventBus.Publish(@event);
        return ValueTask.CompletedTask;
    }
}
