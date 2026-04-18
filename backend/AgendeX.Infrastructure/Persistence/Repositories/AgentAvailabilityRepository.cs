using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendeX.Infrastructure.Persistence.Repositories;

public sealed class AgentAvailabilityRepository : IAgentAvailabilityRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AgentAvailabilityRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AgentAvailability?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.AgentAvailabilities.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<AgentAvailability>> GetByAgentIdAsync(Guid agentId, CancellationToken cancellationToken)
    {
        return await _dbContext.AgentAvailabilities
            .Where(a => a.AgentId == agentId)
            .OrderBy(a => a.WeekDay)
            .ThenBy(a => a.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AgentAvailability>> GetByAgentAndWeekDayAsync(
        Guid agentId, WeekDay weekDay, CancellationToken cancellationToken)
    {
        return await _dbContext.AgentAvailabilities
            .Where(a => a.AgentId == agentId && a.WeekDay == weekDay)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AgentAvailability availability, CancellationToken cancellationToken)
    {
        await _dbContext.AgentAvailabilities.AddAsync(availability, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
