using AgendeX.Application.Common.Interfaces;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AgendeX.WebAPI.Services;

public sealed class AdminSeedService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IOptions<AdminSeedOptions> _options;
    private readonly ILogger<AdminSeedService> _logger;

    public AdminSeedService(
        ApplicationDbContext dbContext,
        IPasswordHasher passwordHasher,
        IOptions<AdminSeedOptions> options,
        ILogger<AdminSeedService> logger)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _options = options;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        AdminSeedOptions options = _options.Value;

        if (!options.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(options.Name) ||
            string.IsNullOrWhiteSpace(options.Email) ||
            string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException(
                "Admin seed is enabled, but one or more required values are missing: AdminSeed:Name, AdminSeed:Email, AdminSeed:Password.");
        }

        string email = options.Email.Trim().ToLowerInvariant();
        User? existingUser = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (existingUser is not null)
        {
            if (existingUser.Role != UserRole.Administrator)
            {
                throw new InvalidOperationException(
                    $"Admin seed aborted: user '{email}' already exists but is not an Administrator.");
            }

            _logger.LogInformation("Admin seed skipped because user '{Email}' already exists.", email);
            return;
        }

        User admin = new(
            options.Name.Trim(),
            email,
            _passwordHasher.Hash(options.Password),
            UserRole.Administrator);

        _dbContext.Users.Add(admin);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Initial admin user '{Email}' created successfully.", email);
    }
}
