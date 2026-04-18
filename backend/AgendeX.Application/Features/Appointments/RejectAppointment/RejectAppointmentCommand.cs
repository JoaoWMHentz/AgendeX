using MediatR;

namespace AgendeX.Application.Features.Appointments;

public sealed record RejectAppointmentCommand(Guid Id, Guid AgentId, string RejectionReason)
    : IRequest<AppointmentDto>;
