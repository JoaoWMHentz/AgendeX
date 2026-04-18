using AgendeX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeX.Infrastructure.Persistence.Configurations;

public class ServiceTypeConfiguration : IEntityTypeConfiguration<ServiceType>
{
    public void Configure(EntityTypeBuilder<ServiceType> builder)
    {
        builder.ToTable("service_types");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Description)
            .HasMaxLength(120)
            .IsRequired();

        builder.HasData(
            new { Id = 1, Description = "Consulting" },
            new { Id = 2, Description = "Technical Support" },
            new { Id = 3, Description = "Commercial Service" },
            new { Id = 4, Description = "Interview" }
        );
    }
}
