using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendeX.Infrastructure.Persistence.Repositories;

public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public AppointmentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Appointments
            .Include(a => a.ServiceType)
            .Include(a => a.Client)
            .Include(a => a.Agent)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetAllAsync(
        Guid? clientId, Guid? agentId, int? serviceTypeId, AppointmentStatus? status,
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        IQueryable<Appointment> query = _dbContext.Appointments
            .Include(a => a.ServiceType)
            .Include(a => a.Client)
            .Include(a => a.Agent);

        if (clientId.HasValue) query = query.Where(a => a.ClientId == clientId.Value);
        if (agentId.HasValue) query = query.Where(a => a.AgentId == agentId.Value);
        if (serviceTypeId.HasValue) query = query.Where(a => a.ServiceTypeId == serviceTypeId.Value);
        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        if (from.HasValue) query = query.Where(a => a.Date >= from.Value);
        if (to.HasValue) query = query.Where(a => a.Date <= to.Value);

        return await query.OrderByDescending(a => a.Date).ThenBy(a => a.Time).ToListAsync(cancellationToken);
    }

    public Task<bool> HasConflictAsync(
        Guid agentId, DateOnly date, TimeOnly time, Guid? excludeId, CancellationToken cancellationToken)
    {
        return _dbContext.Appointments.AnyAsync(a =>
            a.AgentId == agentId &&
            a.Date == date &&
            a.Time == time &&
            a.Id != excludeId &&
            (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.PendingConfirmation),
            cancellationToken);
    }

    public async Task AddAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        await _dbContext.Appointments.AddAsync(appointment, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
