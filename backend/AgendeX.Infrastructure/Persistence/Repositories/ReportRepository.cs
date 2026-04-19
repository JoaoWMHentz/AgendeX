using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendeX.Infrastructure.Persistence.Repositories;

public sealed class ReportRepository : IReportRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ReportRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Appointment>> GetReportAppointmentsAsync(
        IReadOnlyCollection<Guid>? clientIds,
        IReadOnlyCollection<Guid>? agentIds,
        IReadOnlyCollection<int>? serviceTypeIds,
        IReadOnlyCollection<AppointmentStatus>? statuses,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken)
    {
        IQueryable<Appointment> query = _dbContext.Appointments
            .Include(a => a.ServiceType)
            .Include(a => a.Client)
            .Include(a => a.Agent)
            .AsNoTracking();

        if (clientIds is { Count: > 0 })
            query = query.Where(a => clientIds.Contains(a.ClientId));

        if (agentIds is { Count: > 0 })
            query = query.Where(a => agentIds.Contains(a.AgentId));

        if (serviceTypeIds is { Count: > 0 })
            query = query.Where(a => serviceTypeIds.Contains(a.ServiceTypeId));

        if (statuses is { Count: > 0 })
            query = query.Where(a => statuses.Contains(a.Status));

        if (from.HasValue)
            query = query.Where(a => a.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(a => a.Date <= to.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetAgentClientIdsAsync(Guid agentId, CancellationToken cancellationToken)
    {
        return await _dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.AgentId == agentId)
            .Select(a => a.ClientId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }
}
