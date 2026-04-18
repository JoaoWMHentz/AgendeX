using MediatR;

namespace AgendeX.Application.Features.Availability;

public sealed record UpdateAvailabilityRequest(TimeOnly StartTime, TimeOnly EndTime);

public sealed record UpdateAvailabilityCommand(Guid Id, TimeOnly StartTime, TimeOnly EndTime)
    : IRequest<AvailabilityDto>;
