using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Availability;

public sealed record GetAvailabilitiesByAgentQuery(Guid AgentId, WeekDay? WeekDay = null)
    : IRequest<IReadOnlyList<AvailabilityDto>>;

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

        IEnumerable<AgentAvailability> filtered = request.WeekDay.HasValue
            ? slots.Where(s => s.WeekDay == request.WeekDay.Value)
            : slots;

        return filtered.Select(ToDto).ToList().AsReadOnly();
    }

    private static AvailabilityDto ToDto(AgentAvailability a) =>
        new(a.Id, a.AgentId, a.WeekDay, a.StartTime, a.EndTime, a.IsActive);
}
