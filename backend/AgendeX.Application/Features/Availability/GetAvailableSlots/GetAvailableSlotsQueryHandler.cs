using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Availability;

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
