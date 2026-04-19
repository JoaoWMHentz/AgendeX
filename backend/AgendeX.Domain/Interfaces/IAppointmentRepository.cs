using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;

namespace AgendeX.Domain.Interfaces;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Appointment>> GetAllAsync(
        Guid? clientId, Guid? agentId, int? serviceTypeId, AppointmentStatus? status,
        DateOnly? from, DateOnly? to, CancellationToken cancellationToken);
    Task<bool> HasConflictAsync(Guid agentId, DateOnly date, TimeOnly windowStart, TimeOnly windowEnd, Guid? excludeId, CancellationToken cancellationToken);
    Task AddAsync(Appointment appointment, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
