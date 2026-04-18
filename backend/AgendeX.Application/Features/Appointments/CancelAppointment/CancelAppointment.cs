using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Appointments;

public sealed record CancelAppointmentCommand(Guid Id, Guid RequestingUserId, bool IsAdmin)
    : IRequest<AppointmentDto>;

public sealed class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, AppointmentDto>
{
    private readonly IAppointmentRepository _repository;

    public CancelAppointmentCommandHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppointmentDto> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        Appointment appointment = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.Id}' not found.");

        if (request.IsAdmin)
            ValidateAdminCancel(appointment);
        else
            ValidateClientCancel(appointment, request.RequestingUserId);

        appointment.Cancel();
        await _repository.SaveChangesAsync(cancellationToken);

        return AppointmentMapper.ToDto(appointment);
    }

    private static void ValidateAdminCancel(Appointment appointment)
    {
        if (appointment.Status is AppointmentStatus.Completed or AppointmentStatus.Canceled)
            throw new InvalidOperationException("Appointment is already completed or canceled.");
    }

    private static void ValidateClientCancel(Appointment appointment, Guid clientId)
    {
        if (appointment.ClientId != clientId)
            throw new UnauthorizedAccessException("You can only cancel your own appointments.");

        if (appointment.Status is not (AppointmentStatus.PendingConfirmation or AppointmentStatus.Confirmed))
            throw new InvalidOperationException("Appointment cannot be canceled at its current status.");

        if (appointment.Date.ToDateTime(appointment.Time) <= DateTime.UtcNow)
            throw new InvalidOperationException("Cannot cancel an appointment that has already occurred.");
    }
}
