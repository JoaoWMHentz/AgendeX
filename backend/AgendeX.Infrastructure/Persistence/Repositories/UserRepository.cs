using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using AgendeX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgendeX.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _dbContext;

    public UserRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        string normalizedEmail = email.Trim().ToLowerInvariant();

        return _dbContext.Users
            .FirstOrDefaultAsync(user => user.Email.ToLower() == normalizedEmail, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Users
            .Include(user => user.ClientDetail)
            .FirstOrDefaultAsync(user => user.Id == id && user.IsActive, cancellationToken);
    }

    public async Task<IReadOnlyList<User>> GetAllAsync(UserRole? role, CancellationToken cancellationToken)
    {
        IQueryable<User> query = _dbContext.Users
            .Include(user => user.ClientDetail)
            .Where(user => user.IsActive);

        if (role.HasValue)
            query = query.Where(user => user.Role == role.Value);

        return await query.OrderBy(user => user.Name).ToListAsync(cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
