using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Infrastructure.Persistence;
using AgendeX.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AgendeX.Tests.Infrastructure.Persistence;

public sealed class RefreshTokenRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task AddAsync_AndSaveChangesAsync_PersistsToken()
    {
        await using ApplicationDbContext context = CreateContext();
        User user = new("Fernanda", "fernanda@email.com", "hash", UserRole.Client);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        RefreshToken token = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "hash-abc",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        RefreshTokenRepository repository = new(context);
        await repository.AddAsync(token, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        RefreshToken? persisted = await context.RefreshTokens.FindAsync(token.Id);
        persisted.Should().NotBeNull();
        persisted!.TokenHash.Should().Be("hash-abc");
    }

    [Fact]
    public async Task GetByTokenHashAsync_TokenExists_ReturnsTokenWithUser()
    {
        await using ApplicationDbContext context = CreateContext();
        User user = new("Gustavo", "gustavo@email.com", "hash", UserRole.Attendant);
        context.Users.Add(user);

        RefreshToken token = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "hash-xyz",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        RefreshTokenRepository repository = new(context);
        RefreshToken? result = await repository.GetByTokenHashAsync("hash-xyz", CancellationToken.None);

        result.Should().NotBeNull();
        result!.TokenHash.Should().Be("hash-xyz");
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be("gustavo@email.com");
    }

    [Fact]
    public async Task GetByTokenHashAsync_TokenNotFound_ReturnsNull()
    {
        await using ApplicationDbContext context = CreateContext();

        RefreshTokenRepository repository = new(context);
        RefreshToken? result = await repository.GetByTokenHashAsync("nonexistent-hash", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SaveChangesAsync_AfterRevokingToken_PersistsRevocation()
    {
        await using ApplicationDbContext context = CreateContext();
        User user = new("Helena", "helena@email.com", "hash", UserRole.Client);
        context.Users.Add(user);

        RefreshToken token = new()
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = "hash-revoke",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };
        context.RefreshTokens.Add(token);
        await context.SaveChangesAsync();

        token.IsRevoked = true;
        RefreshTokenRepository repository = new(context);
        await repository.SaveChangesAsync(CancellationToken.None);

        RefreshToken? updated = await context.RefreshTokens.FindAsync(token.Id);
        updated!.IsRevoked.Should().BeTrue();
    }
}
