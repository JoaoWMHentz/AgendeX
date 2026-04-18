using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendeX.Infrastructure.Persistence.Repositories;

public sealed class ServiceTypeRepository : IServiceTypeRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ServiceTypeRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ServiceType?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _dbContext.ServiceTypes.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<ServiceType>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.ServiceTypes
            .OrderBy(s => s.Description)
            .ToListAsync(cancellationToken);
    }
}
