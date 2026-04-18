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

    // ClientId vem do token — só os campos do body
    public sealed record CreateAppointmentBody(
        string Title, string? Description, int ServiceTypeId,
        Guid AgentId, DateOnly Date, TimeOnly Time, string? Notes);

    // Id vem da rota — só o campo do body
    public sealed record RejectBody(string RejectionReason);
    public sealed record CompleteBody(string? ServiceSummary);
    public sealed record ReassignBody(Guid NewAgentId);

    private Guid CurrentUserId => _currentUser.UserId;
    private bool IsAdmin => _currentUser.Role == UserRole.Administrator;
    private bool IsAgent => _currentUser.Role == UserRole.Agent;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<AppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? clientId, [FromQuery] Guid? agentId,
        [FromQuery] int? serviceTypeId, [FromQuery] AppointmentStatus? status,
        [FromQuery] DateOnly? from, [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        Guid? filterClientId = IsAdmin ? clientId : (IsAgent ? null : CurrentUserId);
        Guid? filterAgentId = IsAdmin ? agentId : (IsAgent ? CurrentUserId : agentId);

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
        [FromBody] CreateAppointmentBody body, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(new CreateAppointmentCommand(
            body.Title, body.Description, body.ServiceTypeId,
            CurrentUserId, body.AgentId, body.Date, body.Time, body.Notes),
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
            new ConfirmAppointmentCommand(id, CurrentUserId), cancellationToken);
        return Ok(appointment);
    }

    [HttpPut("{id:guid}/reject")]
    [Authorize(Roles = Roles.Agent)]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(
        Guid id, [FromBody] RejectBody body, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(
            new RejectAppointmentCommand(id, CurrentUserId, body.RejectionReason), cancellationToken);
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
            new CancelAppointmentCommand(id, CurrentUserId, IsAdmin), cancellationToken);
        return Ok(appointment);
    }

    [HttpPut("{id:guid}/complete")]
    [Authorize(Roles = Roles.Agent)]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        Guid id, [FromBody] CompleteBody body, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(
            new CompleteAppointmentCommand(id, CurrentUserId, body.ServiceSummary), cancellationToken);
        return Ok(appointment);
    }

    [HttpPut("{id:guid}/reassign")]
    [Authorize(Roles = Roles.Administrator)]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reassign(
        Guid id, [FromBody] ReassignBody body, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(
            new ReassignAppointmentCommand(id, body.NewAgentId), cancellationToken);
        return Ok(appointment);
    }
}
