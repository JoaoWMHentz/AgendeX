using AgendeX.Domain.Enums;
using MediatR;

namespace AgendeX.Application.Features.Appointments;

public sealed record GetAppointmentsQuery(
    Guid? ClientId,
    Guid? AgentId,
    int? ServiceTypeId,
    AppointmentStatus? Status,
    DateOnly? From,
    DateOnly? To
) : IRequest<IReadOnlyList<AppointmentDto>>;
