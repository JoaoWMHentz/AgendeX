using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;

namespace AgendeX.Application.Features.Availability;

internal sealed class AvailabilityCreationService
{
    private readonly IAgentAvailabilityRepository _repository;
    private readonly IUserRepository _userRepository;

    public AvailabilityCreationService(
        IAgentAvailabilityRepository repository,
        IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<AvailabilityDto>> CreateForWeekDaysAsync(
        Guid agentId,
        IReadOnlyList<WeekDay> weekDays,
        TimeOnly startTime,
        TimeOnly endTime,
        int? slotDurationMinutes,
        CancellationToken cancellationToken)
    {
        await EnsureAgentExistsAsync(agentId, cancellationToken);

        List<(TimeOnly Start, TimeOnly End)> slots = BuildSlots(startTime, endTime, slotDurationMinutes);
        List<WeekDay> distinctWeekDays = weekDays.Distinct().ToList();
        List<AgentAvailability> createdAvailabilities = [];

        foreach (WeekDay weekDay in distinctWeekDays)
        {
            foreach ((TimeOnly slotStart, TimeOnly slotEnd) in slots)
            {
                await EnsureNoOverlapAsync(agentId, weekDay, slotStart, slotEnd, cancellationToken);

                AgentAvailability availability = new(agentId, weekDay, slotStart, slotEnd);
                createdAvailabilities.Add(availability);
                await _repository.AddAsync(availability, cancellationToken);
            }
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return createdAvailabilities.Select(ToDto).ToList().AsReadOnly();
    }

    private static List<(TimeOnly Start, TimeOnly End)> BuildSlots(
        TimeOnly startTime, TimeOnly endTime, int? slotDurationMinutes)
    {
        if (slotDurationMinutes is null)
            return [(startTime, endTime)];

        List<(TimeOnly, TimeOnly)> slots = [];
        TimeOnly current = startTime;

        while (current < endTime)
        {
            TimeOnly next = current.AddMinutes(slotDurationMinutes.Value);
            if (next > endTime) break;
            slots.Add((current, next));
            current = next;
        }

        return slots;
    }

    private async Task EnsureAgentExistsAsync(Guid agentId, CancellationToken cancellationToken)
    {
        User? agent = await _userRepository.GetByIdAsync(agentId, cancellationToken);
        if (agent is null || agent.Role != UserRole.Agent)
            throw new KeyNotFoundException($"Atendente '{agentId}' nao encontrado.");
    }

    private async Task EnsureNoOverlapAsync(
        Guid agentId,
        WeekDay weekDay,
        TimeOnly start,
        TimeOnly end,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<AgentAvailability> existing =
            await _repository.GetByAgentAndWeekDayAsync(agentId, weekDay, cancellationToken);

        bool hasOverlap = existing.Any(a =>
            a.IsActive && start < a.EndTime && end > a.StartTime);

        if (hasOverlap)
            throw new InvalidOperationException(
                $"Conflito de disponibilidade na {GetWeekDayLabel(weekDay)}. Ja existe intervalo para esse horario.");
    }

    private static string GetWeekDayLabel(WeekDay weekDay)
    {
        return weekDay switch
        {
            WeekDay.Monday => "segunda-feira",
            WeekDay.Tuesday => "terca-feira",
            WeekDay.Wednesday => "quarta-feira",
            WeekDay.Thursday => "quinta-feira",
            WeekDay.Friday => "sexta-feira",
            WeekDay.Saturday => "sabado",
            WeekDay.Sunday => "domingo",
            _ => weekDay.ToString()
        };
    }

    private static AvailabilityDto ToDto(AgentAvailability availability)
    {
        return new AvailabilityDto(
            availability.Id,
            availability.AgentId,
            availability.WeekDay,
            availability.StartTime,
            availability.EndTime,
            availability.IsActive);
    }
}
