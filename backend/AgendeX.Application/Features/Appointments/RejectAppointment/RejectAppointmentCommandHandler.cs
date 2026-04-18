using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Appointments;

public sealed class RejectAppointmentCommandHandler : IRequestHandler<RejectAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _repository;

    public RejectAppointmentCommandHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppointmentDto> Handle(RejectAppointmentCommand request, CancellationToken cancellationToken)
    {
        Appointment appointment = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.Id}' not found.");

        if (appointment.AgentId != request.AgentId)
            throw new UnauthorizedAccessException("You are not the assigned agent for this appointment.");

        if (appointment.Status != AppointmentStatus.PendingConfirmation)
            throw new InvalidOperationException("Only pending appointments can be rejected.");

        appointment.Reject(request.RejectionReason);
        await _repository.SaveChangesAsync(cancellationToken);

        return AppointmentMapper.ToDto(appointment);
    }
}
