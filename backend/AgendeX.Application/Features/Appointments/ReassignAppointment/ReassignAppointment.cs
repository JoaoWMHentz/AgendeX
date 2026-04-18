using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Appointments;

public sealed record ReassignAppointmentCommand(Guid Id, Guid NewAgentId) : IRequest<AppointmentDto>;

public sealed class ReassignAppointmentCommandHandler : IRequestHandler<ReassignAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IUserRepository _userRepository;

    public ReassignAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IUserRepository userRepository)
    {
        _appointmentRepository = appointmentRepository;
        _userRepository = userRepository;
    }

    public async Task<AppointmentDto> Handle(ReassignAppointmentCommand request, CancellationToken cancellationToken)
    {
        Appointment appointment = await _appointmentRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.Id}' not found.");

        User? newAgent = await _userRepository.GetByIdAsync(request.NewAgentId, cancellationToken);
        if (newAgent is null || newAgent.Role != UserRole.Agent)
            throw new KeyNotFoundException($"Agent '{request.NewAgentId}' not found.");

        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Canceled)
            throw new InvalidOperationException("Cannot reassign a completed or canceled appointment.");

        appointment.Reassign(request.NewAgentId);
        await _appointmentRepository.SaveChangesAsync(cancellationToken);

        Appointment updated = await _appointmentRepository.GetByIdAsync(appointment.Id, cancellationToken) ?? appointment;
        return AppointmentMapper.ToDto(updated);
    }
}
