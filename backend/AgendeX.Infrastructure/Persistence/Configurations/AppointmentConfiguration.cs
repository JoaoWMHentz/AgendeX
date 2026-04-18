using AgendeX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeX.Infrastructure.Persistence.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Title).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Description).HasMaxLength(1000);
        builder.Property(a => a.RejectionReason).HasMaxLength(500);
        builder.Property(a => a.ServiceSummary).HasMaxLength(2000);
        builder.Property(a => a.Notes).HasMaxLength(1000);
        builder.Property(a => a.Status).IsRequired();
        builder.Property(a => a.Date).IsRequired();
        builder.Property(a => a.Time).IsRequired();
        builder.Property(a => a.CreatedAt).IsRequired();

        builder.HasOne(a => a.ServiceType)
            .WithMany()
            .HasForeignKey(a => a.ServiceTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Client)
            .WithMany()
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Agent)
            .WithMany()
            .HasForeignKey(a => a.AgentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
