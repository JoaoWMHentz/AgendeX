using AgendeX.Domain.Entities;

namespace AgendeX.Domain.Interfaces;

public interface IClientDetailRepository
{
    Task<ClientDetail?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(ClientDetail clientDetail, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
