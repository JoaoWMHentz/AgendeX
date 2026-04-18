using AgendeX.Domain.Enums;

namespace AgendeX.Application.Features.Availability;

public sealed record AvailabilityDto(
    Guid Id,
    Guid AgentId,
    WeekDay WeekDay,
    TimeOnly StartTime,
    TimeOnly EndTime,
    bool IsActive);

public sealed record AvailableSlotDto(TimeOnly StartTime, TimeOnly EndTime);
