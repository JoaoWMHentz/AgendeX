using AgendeX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgendeX.Infrastructure.Data;

public class AgendeXDbContext : DbContext
{
    public AgendeXDbContext(DbContextOptions<AgendeXDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ClientDetail> ClientDetails => Set<ClientDetail>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AgendeXDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
