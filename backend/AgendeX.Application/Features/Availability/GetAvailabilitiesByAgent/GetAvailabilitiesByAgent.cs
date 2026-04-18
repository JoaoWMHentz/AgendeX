using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Availability;

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
