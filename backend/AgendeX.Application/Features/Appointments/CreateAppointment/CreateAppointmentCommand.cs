using MediatR;

namespace AgendeX.Application.Features.Appointments;

public sealed record CreateAppointmentRequest(
    string Title,
    string? Description,
    int ServiceTypeId,
    Guid AgentId,
    DateOnly Date,
    TimeOnly Time,
    string? Notes);

public sealed record CreateAppointmentCommand(
    string Title,
    string? Description,
    int ServiceTypeId,
    Guid ClientId,
    Guid AgentId,
    DateOnly Date,
    TimeOnly Time,
    string? Notes
) : IRequest<AppointmentDto>;
