using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Appointments;
public sealed record CompleteAppointmentCommand(Guid Id, Guid AgentId, string? ServiceSummary)
    : IRequest<AppointmentDto>;

public sealed class CompleteAppointmentCommandHandler : IRequestHandler<CompleteAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _repository;

    public CompleteAppointmentCommandHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppointmentDto> Handle(CompleteAppointmentCommand request, CancellationToken cancellationToken)
    {
        Appointment appointment = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.Id}' not found.");

        if (appointment.AgentId != request.AgentId)
            throw new UnauthorizedAccessException("You are not the assigned agent for this appointment.");

        if (appointment.Status != AppointmentStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed appointments can be marked as completed.");

        if (appointment.Date.ToDateTime(appointment.Time) > DateTime.UtcNow)
            throw new InvalidOperationException("Cannot complete an appointment that has not occurred yet.");

        appointment.Complete(request.ServiceSummary);
        await _repository.SaveChangesAsync(cancellationToken);

        return AppointmentMapper.ToDto(appointment);
    }
}
