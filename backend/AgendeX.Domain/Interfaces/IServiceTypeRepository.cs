using AgendeX.Domain.Entities;

namespace AgendeX.Domain.Interfaces;

public interface IServiceTypeRepository
{
    Task<ServiceType?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ServiceType>> GetAllAsync(CancellationToken cancellationToken);
}
