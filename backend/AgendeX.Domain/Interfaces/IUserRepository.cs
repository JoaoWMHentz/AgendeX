using AgendeX.Domain.Entities;

namespace AgendeX.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
}
