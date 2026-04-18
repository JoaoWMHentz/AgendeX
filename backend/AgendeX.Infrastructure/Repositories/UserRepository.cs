using AgendeX.Domain.Entities;
using AgendeX.Domain.Interfaces;
using AgendeX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgendeX.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly AgendeXDbContext _dbContext;

    public UserRepository(AgendeXDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();

        return _dbContext.Users
            .FirstOrDefaultAsync(user => user.Email.ToLower() == normalizedEmail, cancellationToken);
    }
}
