using AgendeX.Domain.Enums;
using MediatR;

namespace AgendeX.Application.Features.Availability;

public sealed record CreateAvailabilityCommand(
    Guid AgentId,
    IReadOnlyList<WeekDay> WeekDays,
    TimeOnly StartTime,
    TimeOnly EndTime
) : IRequest<IReadOnlyList<AvailabilityDto>>;
