using NSubstitute;
using mqonnor.Application.DTOs;
using mqonnor.Application.Mappers;
using mqonnor.Application.Messaging;
using mqonnor.Application.UseCases.Event;
using mqonnor.Domain.Entities;

namespace mqonnor.Tests.Application;

public class PublishEventCommandHandlerTests
{
    private readonly IMapper<PublishEventDto, Event> _mapper = Substitute.For<IMapper<PublishEventDto, Event>>();
    private readonly IEventBus _eventBus = Substitute.For<IEventBus>();
    private readonly PublishEventCommandHandler _sut;

    private static readonly byte[] Payload = [1, 2, 3];
    private static readonly PublishEventDto Dto = new(Payload, "UTF-8", "test");
    private static readonly Event MappedEvent = new(Guid.NewGuid(), Payload, new EventMetainfo("UTF-8", 3, "test"));

    public PublishEventCommandHandlerTests()
    {
        _mapper.Map(Dto).Returns(MappedEvent);
        _sut = new PublishEventCommandHandler(_mapper, _eventBus);
    }

    [Fact]
    public async Task HandleAsync_Success_ReturnsSuccessResult()
    {
        var result = await _sut.HandleAsync(new PublishEventCommand(Dto));

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_Success_PublishesEventTobus()
    {
        await _sut.HandleAsync(new PublishEventCommand(Dto));

        await _eventBus.Received(1).PublishAsync(MappedEvent, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Success_MapsDto()
    {
        await _sut.HandleAsync(new PublishEventCommand(Dto));

        _mapper.Received(1).Map(Dto);
    }

    [Fact]
    public async Task HandleAsync_CancelledToken_ReturnsFailureResult()
    {
        var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _eventBus.PublishAsync(Arg.Any<Event>(), Arg.Is<CancellationToken>(ct => ct.IsCancellationRequested))
            .Returns(_ => ValueTask.FromException(new OperationCanceledException()));

        var result = await _sut.HandleAsync(new PublishEventCommand(Dto), cts.Token);

        Assert.True(result.IsFailure);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task HandleAsync_BusThrowsOperationCancelled_ReturnsFailureResult()
    {
        _eventBus.PublishAsync(Arg.Any<Event>(), Arg.Any<CancellationToken>())
            .Returns(_ => ValueTask.FromException(new OperationCanceledException()));

        var result = await _sut.HandleAsync(new PublishEventCommand(Dto));

        Assert.True(result.IsFailure);
    }
}
