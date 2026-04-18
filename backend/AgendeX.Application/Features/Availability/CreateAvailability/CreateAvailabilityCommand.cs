using AgendeX.Domain.Enums;
using MediatR;

namespace AgendeX.Application.Features.Availability;

public sealed record CreateAvailabilityCommand(
    Guid AgentId,
    WeekDay WeekDay,
    TimeOnly StartTime,
    TimeOnly EndTime
) : IRequest<AvailabilityDto>;
