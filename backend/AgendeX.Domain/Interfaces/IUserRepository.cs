using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;

namespace AgendeX.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<User>> GetAllAsync(UserRole? role, CancellationToken cancellationToken);
    Task<IReadOnlyList<User>> GetActiveAgentsAsync(CancellationToken cancellationToken);
    Task AddAsync(User user, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
