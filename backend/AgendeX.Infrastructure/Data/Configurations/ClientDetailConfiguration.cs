using AgendeX.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendeX.Infrastructure.Data.Configurations;

public class ClientDetailConfiguration : IEntityTypeConfiguration<ClientDetail>
{
    public void Configure(EntityTypeBuilder<ClientDetail> builder)
    {
        builder.ToTable("client_details");

        builder.HasKey(clientDetail => clientDetail.Id);

        builder.Property(clientDetail => clientDetail.CPF)
            .HasMaxLength(14)
            .IsRequired();

        builder.Property(clientDetail => clientDetail.BirthDate)
            .IsRequired();

        builder.Property(clientDetail => clientDetail.Phone)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(clientDetail => clientDetail.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(clientDetail => clientDetail.UserId)
            .IsUnique();

        builder.HasIndex(clientDetail => clientDetail.CPF)
            .IsUnique();

        builder.HasOne(clientDetail => clientDetail.User)
            .WithOne()
            .HasForeignKey<ClientDetail>(clientDetail => clientDetail.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
