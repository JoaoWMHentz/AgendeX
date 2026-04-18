using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AgendeX.Infrastructure.Persistence.Repositories;

public sealed class ClientDetailRepository : IClientDetailRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ClientDetailRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ClientDetail?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _dbContext.ClientDetails
            .FirstOrDefaultAsync(cd => cd.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(ClientDetail clientDetail, CancellationToken cancellationToken)
    {
        await _dbContext.ClientDetails.AddAsync(clientDetail, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
