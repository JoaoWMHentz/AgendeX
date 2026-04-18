using AgendeX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeX.Infrastructure.Persistence.Configurations;

public class AgentAvailabilityConfiguration : IEntityTypeConfiguration<AgentAvailability>
{
    public void Configure(EntityTypeBuilder<AgentAvailability> builder)
    {
        builder.ToTable("agent_availabilities");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.WeekDay).IsRequired();
        builder.Property(a => a.StartTime).IsRequired();
        builder.Property(a => a.EndTime).IsRequired();
        builder.Property(a => a.IsActive).IsRequired();

        builder.HasOne(a => a.Agent)
            .WithMany()
            .HasForeignKey(a => a.AgentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
