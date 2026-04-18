using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;

namespace AgendeX.Domain.Interfaces;

public interface IAgentAvailabilityRepository
{
    Task<AgentAvailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<AgentAvailability>> GetByAgentIdAsync(Guid agentId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AgentAvailability>> GetByAgentAndWeekDayAsync(Guid agentId, WeekDay weekDay, CancellationToken cancellationToken);
    Task AddAsync(AgentAvailability availability, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
