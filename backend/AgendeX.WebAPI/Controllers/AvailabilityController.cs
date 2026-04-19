using AgendeX.Application.Features.Availability;
using AgendeX.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendeX.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AvailabilityController : ControllerBase
{
    private readonly ISender _sender;

    public AvailabilityController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("agent/{agentId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AvailabilityDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByAgent(Guid agentId, CancellationToken cancellationToken)
    {
        IReadOnlyList<AvailabilityDto> slots =
            await _sender.Send(new GetAvailabilitiesByAgentQuery(agentId), cancellationToken);
        return Ok(slots);
    }

    [HttpGet("slots")]
    [ProducesResponseType(typeof(IReadOnlyList<AvailableSlotDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableSlots(
        [FromQuery] Guid agentId, [FromQuery] DateOnly date, CancellationToken cancellationToken)
    {
        IReadOnlyList<AvailableSlotDto> slots =
            await _sender.Send(new GetAvailableSlotsQuery(agentId, date), cancellationToken);
        return Ok(slots);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType(typeof(IReadOnlyList<AvailabilityDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAvailabilityCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyList<AvailabilityDto> result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByAgent), new { agentId = command.AgentId }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType(typeof(AvailabilityDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id, [FromBody] UpdateAvailabilityRequest body, CancellationToken cancellationToken)
    {
        AvailabilityDto result = await _sender.Send(
            new UpdateAvailabilityCommand(id, body.StartTime, body.EndTime), cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _sender.Send(new DeleteAvailabilityCommand(id), cancellationToken);
        return NoContent();
    }
}
