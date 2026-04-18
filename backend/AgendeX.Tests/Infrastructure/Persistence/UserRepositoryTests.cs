using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Infrastructure.Persistence;
using AgendeX.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AgendeX.Tests.Infrastructure.Persistence;

public sealed class UserRepositoryTests
{
    private static ApplicationDbContext CreateContext()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    // ── GetByEmailAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task GetByEmailAsync_UserExists_ReturnsUser()
    {
        await using ApplicationDbContext context = CreateContext();
        User user = new("Ana", "ana@email.com", "hash", UserRole.Client);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        UserRepository repository = new(context);
        User? result = await repository.GetByEmailAsync("ana@email.com", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Email.Should().Be("ana@email.com");
    }

    [Fact]
    public async Task GetByEmailAsync_EmailNotFound_ReturnsNull()
    {
        await using ApplicationDbContext context = CreateContext();

        UserRepository repository = new(context);
        User? result = await repository.GetByEmailAsync("notfound@email.com", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_DifferentCase_ReturnsUser()
    {
        await using ApplicationDbContext context = CreateContext();
        User user = new("Carlos", "carlos@email.com", "hash", UserRole.Agent);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        UserRepository repository = new(context);
        User? result = await repository.GetByEmailAsync("CARLOS@EMAIL.COM", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Carlos");
    }

    [Fact]
    public async Task GetByEmailAsync_EmailWithLeadingAndTrailingSpaces_NormalizesAndReturnsUser()
    {
        await using ApplicationDbContext context = CreateContext();
        User user = new("Diana", "diana@email.com", "hash", UserRole.Client);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        UserRepository repository = new(context);
        User? result = await repository.GetByEmailAsync("  diana@email.com  ", CancellationToken.None);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Diana");
    }

    // ── GetByIdAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ActiveUser_ReturnsUser()
    {
        await using ApplicationDbContext context = CreateContext();
        User user = new("Eva", "eva@email.com", "hash", UserRole.Client);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        UserRepository repository = new(context);
        User? result = await repository.GetByIdAsync(user.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task GetByIdAsync_InactiveUser_ReturnsNull()
    {
        await using ApplicationDbContext context = CreateContext();
        User user = new("Felipe", "felipe@email.com", "hash", UserRole.Client);
        user.Deactivate();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        UserRepository repository = new(context);
        User? result = await repository.GetByIdAsync(user.Id, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ReturnsNull()
    {
        await using ApplicationDbContext context = CreateContext();

        UserRepository repository = new(context);
        User? result = await repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    // ── GetAllAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveUsers()
    {
        await using ApplicationDbContext context = CreateContext();
        User active = new("Gabi", "gabi@email.com", "hash", UserRole.Client);
        User inactive = new("Hugo", "hugo@email.com", "hash", UserRole.Client);
        inactive.Deactivate();
        context.Users.AddRange(active, inactive);
        await context.SaveChangesAsync();

        UserRepository repository = new(context);
        IReadOnlyList<User> result = await repository.GetAllAsync(null, CancellationToken.None);

        result.Should().ContainSingle(u => u.Id == active.Id);
        result.Should().NotContain(u => u.Id == inactive.Id);
    }

    [Fact]
    public async Task GetAllAsync_WithRoleFilter_ReturnsOnlyMatchingRole()
    {
        await using ApplicationDbContext context = CreateContext();
        User client = new("Ines", "ines@email.com", "hash", UserRole.Client);
        User attendant = new("Jorge", "jorge@email.com", "hash", UserRole.Agent);
        context.Users.AddRange(client, attendant);
        await context.SaveChangesAsync();

        UserRepository repository = new(context);
        IReadOnlyList<User> result = await repository.GetAllAsync(UserRole.Client, CancellationToken.None);

        result.Should().ContainSingle(u => u.Id == client.Id);
        result.Should().NotContain(u => u.Id == attendant.Id);
    }

    // ── AddAsync + SaveChangesAsync ────────────────────────────────────────

    [Fact]
    public async Task AddAsync_AndSaveChangesAsync_PersistsUser()
    {
        await using ApplicationDbContext context = CreateContext();
        User user = new("Karen", "karen@email.com", "hash", UserRole.Administrator);

        UserRepository repository = new(context);
        await repository.AddAsync(user, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        User? persisted = await context.Users.FindAsync(user.Id);
        persisted.Should().NotBeNull();
        persisted!.Email.Should().Be("karen@email.com");
    }
}
