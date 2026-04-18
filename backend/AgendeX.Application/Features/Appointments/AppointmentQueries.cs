using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Appointments;

// ── GetAppointments ─────────────────────────────────────────────────────────

public sealed record GetAppointmentsQuery(
    Guid? ClientId,
    Guid? AgentId,
    int? ServiceTypeId,
    AppointmentStatus? Status,
    DateOnly? From,
    DateOnly? To
) : IRequest<IReadOnlyList<AppointmentDto>>;

public sealed class GetAppointmentsQueryHandler
    : IRequestHandler<GetAppointmentsQuery, IReadOnlyList<AppointmentDto>>
{
    private readonly IAppointmentRepository _repository;

    public GetAppointmentsQueryHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AppointmentDto>> Handle(
        GetAppointmentsQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<Appointment> appointments = await _repository.GetAllAsync(
            request.ClientId, request.AgentId, request.ServiceTypeId,
            request.Status, request.From, request.To, cancellationToken);

        return appointments.Select(AppointmentMapper.ToDto).ToList().AsReadOnly();
    }
}

// ── GetAppointmentById ──────────────────────────────────────────────────────

public sealed record GetAppointmentByIdQuery(Guid Id) : IRequest<AppointmentDto>;

public sealed class GetAppointmentByIdQueryHandler : IRequestHandler<GetAppointmentByIdQuery, AppointmentDto>
{
    private readonly IAppointmentRepository _repository;

    public GetAppointmentByIdQueryHandler(IAppointmentRepository repository)
    {
        _repository = repository;
    }

    public async Task<AppointmentDto> Handle(GetAppointmentByIdQuery request, CancellationToken cancellationToken)
    {
        Appointment appointment = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Appointment '{request.Id}' not found.");

        return AppointmentMapper.ToDto(appointment);
    }
}
