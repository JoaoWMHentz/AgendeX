using AgendeX.Application.Common.Interfaces;
using AgendeX.Application.Features.Appointments;
using AgendeX.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendeX.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AppointmentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ICurrentUserService _currentUser;

    public AppointmentsController(ISender sender, ICurrentUserService currentUser)
    {
        _sender = sender;
        _currentUser = currentUser;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? clientId, [FromQuery] Guid? agentId,
        [FromQuery] int? serviceTypeId, [FromQuery] AppointmentStatus? status,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        Guid? filterClientId = _currentUser.IsAdmin ? clientId : (_currentUser.IsAgent ? null : _currentUser.UserId);
        Guid? filterAgentId = _currentUser.IsAdmin ? agentId : (_currentUser.IsAgent ? _currentUser.UserId : agentId);

        IReadOnlyList<AppointmentDto> appointments = await _sender.Send(
            new GetAppointmentsQuery(filterClientId, filterAgentId, serviceTypeId, status, from, to),
            cancellationToken);

        return Ok(appointments);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(new GetAppointmentByIdQuery(id), cancellationToken);
        return Ok(appointment);
    }

    [HttpPost]
    [Authorize(Roles = Roles.Client)]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAppointmentRequest body, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(new CreateAppointmentCommand(
            body.Title, body.Description, body.ServiceTypeId,
            _currentUser.UserId, body.AgentId, body.Date, body.Time, body.Notes),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id:guid}/confirm")]
    [Authorize(Roles = Roles.Agent)]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Confirm(Guid id, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(
            new ConfirmAppointmentCommand(id, _currentUser.UserId), cancellationToken);
        return Ok(appointment);
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = Roles.Agent)]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(
        Guid id, [FromBody] string rejectionReason, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(
            new RejectAppointmentCommand(id, _currentUser.UserId, rejectionReason), cancellationToken);
        return Ok(appointment);
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize(Roles = $"{Roles.Client},{Roles.Administrator}")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(
            new CancelAppointmentCommand(id, _currentUser.UserId, _currentUser.IsAdmin), cancellationToken);
        return Ok(appointment);
    }

    [HttpPut("{id:guid}/complete")]
    [Authorize(Roles = Roles.Agent)]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        Guid id, [FromBody] string? serviceSummary, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(
            new CompleteAppointmentCommand(id, _currentUser.UserId, serviceSummary), cancellationToken);
        return Ok(appointment);
    }

    [HttpPut("{id:guid}/reassign")]
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reassign(
        Guid id, [FromBody] Guid newAgentId, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(
            new ReassignAppointmentCommand(id, newAgentId), cancellationToken);
        return Ok(appointment);
    }
}
