using AgendeX.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgendeX.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<ClientDetail> ClientDetails => Set<ClientDetail>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
    public DbSet<AgentAvailability> AgentAvailabilities => Set<AgentAvailability>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
