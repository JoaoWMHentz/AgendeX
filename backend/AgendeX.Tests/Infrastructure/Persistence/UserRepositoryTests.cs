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
        User user = new("Carlos", "carlos@email.com", "hash", UserRole.Attendant);
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
}
