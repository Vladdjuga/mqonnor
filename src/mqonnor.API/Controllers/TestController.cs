using Microsoft.AspNetCore.Mvc;
using mqonnor.Application.Abstractions;
using mqonnor.Application.DTOs;

namespace mqonnor.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TestController(IMediator mediator) : ControllerBase
{
    [HttpPost("publish")]
    public async Task<IActionResult> Publish([FromBody] PublishEventDto dto, CancellationToken cancellationToken)
    {
        // TODO: create a PublishEventCommand from dto and send via mediator
        // await mediator.NotifyAsync(new PublishEventCommand(dto), cancellationToken);
        return Accepted();
    }
}
