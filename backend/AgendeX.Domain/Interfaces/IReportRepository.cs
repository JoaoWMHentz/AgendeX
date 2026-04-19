using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;

namespace AgendeX.Domain.Interfaces;

public interface IReportRepository
{
    Task<IReadOnlyList<Appointment>> GetReportAppointmentsAsync(
        IReadOnlyCollection<Guid>? clientIds,
        IReadOnlyCollection<Guid>? agentIds,
        IReadOnlyCollection<int>? serviceTypeIds,
        IReadOnlyCollection<AppointmentStatus>? statuses,
        DateOnly? from,
        DateOnly? to,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Guid>> GetAgentClientIdsAsync(Guid agentId, CancellationToken cancellationToken);
}