using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Availability;

// ── GetAvailabilitiesByAgent ────────────────────────────────────────────────

public sealed record GetAvailabilitiesByAgentQuery(Guid AgentId) : IRequest<IReadOnlyList<AvailabilityDto>>;

public sealed class GetAvailabilitiesByAgentQueryHandler
    : IRequestHandler<GetAvailabilitiesByAgentQuery, IReadOnlyList<AvailabilityDto>>
{
    private readonly IAgentAvailabilityRepository _repository;

    public GetAvailabilitiesByAgentQueryHandler(IAgentAvailabilityRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<AvailabilityDto>> Handle(
        GetAvailabilitiesByAgentQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<AgentAvailability> slots =
            await _repository.GetByAgentIdAsync(request.AgentId, cancellationToken);

        return slots.Select(ToDto).ToList().AsReadOnly();
    }

    private static AvailabilityDto ToDto(AgentAvailability a) =>
        new(a.Id, a.AgentId, a.WeekDay, a.StartTime, a.EndTime, a.IsActive);
}

// ── GetAvailableSlots ───────────────────────────────────────────────────────

public sealed record GetAvailableSlotsQuery(Guid AgentId, DateOnly Date)
    : IRequest<IReadOnlyList<AvailableSlotDto>>;

public sealed class GetAvailableSlotsQueryHandler
    : IRequestHandler<GetAvailableSlotsQuery, IReadOnlyList<AvailableSlotDto>>
{
    private readonly IAgentAvailabilityRepository _availabilityRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public GetAvailableSlotsQueryHandler(
        IAgentAvailabilityRepository availabilityRepository,
        IAppointmentRepository appointmentRepository)
    {
        _availabilityRepository = availabilityRepository;
        _appointmentRepository = appointmentRepository;
    }

    public async Task<IReadOnlyList<AvailableSlotDto>> Handle(
        GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        WeekDay weekDay = (WeekDay)request.Date.DayOfWeek;

        IReadOnlyList<AgentAvailability> slots = await _availabilityRepository
            .GetByAgentAndWeekDayAsync(request.AgentId, weekDay, cancellationToken);

        HashSet<TimeOnly> occupiedTimes = await GetOccupiedTimesAsync(request.AgentId, request.Date, cancellationToken);

        return slots
            .Where(s => s.IsActive && !occupiedTimes.Contains(s.StartTime))
            .Select(s => new AvailableSlotDto(s.StartTime, s.EndTime))
            .ToList()
            .AsReadOnly();
    }

    private async Task<HashSet<TimeOnly>> GetOccupiedTimesAsync(
        Guid agentId, DateOnly date, CancellationToken cancellationToken)
    {
        IReadOnlyList<Appointment> occupied = await _appointmentRepository
            .GetAllAsync(null, agentId, null, null, date, date, cancellationToken);

        return occupied
            .Where(a => a.Status is AppointmentStatus.Confirmed or AppointmentStatus.PendingConfirmation)
            .Select(a => a.Time)
            .ToHashSet();
    }
}
