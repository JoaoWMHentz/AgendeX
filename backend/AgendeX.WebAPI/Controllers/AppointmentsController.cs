using AgendeX.Application.Features.Appointments;
using AgendeX.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgendeX.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class AppointmentsController : ControllerBase
{
    private readonly ISender _sender;

    public AppointmentsController(ISender sender)
    {
        _sender = sender;
    }

    public sealed record CreateAppointmentRequest(
        string Title, string? Description, int ServiceTypeId,
        Guid AgentId, DateOnly Date, TimeOnly Time, string? Notes);

    public sealed record RejectAppointmentRequest(string RejectionReason);

    public sealed record CompleteAppointmentRequest(string? ServiceSummary);

    public sealed record ReassignAppointmentRequest(Guid NewAgentId);

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue("sub") ?? throw new UnauthorizedAccessException("User not authenticated."));

    private bool IsAdmin =>
        User.IsInRole("Administrator");

    private bool IsAgent =>
        User.IsInRole("Agent");

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
    [Authorize(Roles = "Client")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAppointmentRequest request, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(new CreateAppointmentCommand(
            request.Title, request.Description, request.ServiceTypeId,
            CurrentUserId, request.AgentId, request.Date, request.Time, request.Notes),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    [HttpPut("{id:guid}/confirm")]
    [Authorize(Roles = "Agent")]
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
    [Authorize(Roles = "Agent")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(
        Guid id, [FromBody] RejectAppointmentRequest request, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(
            new RejectAppointmentCommand(id, CurrentUserId, request.RejectionReason), cancellationToken);
        return Ok(appointment);
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize(Roles = "Client,Administrator")]
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
    [Authorize(Roles = "Agent")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        Guid id, [FromBody] CompleteAppointmentRequest request, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(
            new CompleteAppointmentCommand(id, CurrentUserId, request.ServiceSummary), cancellationToken);
        return Ok(appointment);
    }

    [HttpPut("{id:guid}/reassign")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(AppointmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reassign(
        Guid id, [FromBody] ReassignAppointmentRequest request, CancellationToken cancellationToken)
    {
        AppointmentDto appointment = await _sender.Send(
            new ReassignAppointmentCommand(id, request.NewAgentId), cancellationToken);
        return Ok(appointment);
    }
}
