using AgendeX.Domain.Entities;
using AgendeX.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace AgendeX.Tests.Infrastructure.Persistence;

public sealed class EntityConfigurationTests
{
    private static IModel CreateModel()
    {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using ApplicationDbContext context = new(options);
        return context.Model;
    }

    // --- User ---

    [Fact]
    public void UserConfiguration_Email_HasMaxLength180()
    {
        IModel model = CreateModel();
        IProperty property = model.FindEntityType(typeof(User))!.FindProperty(nameof(User.Email))!;

        property.GetMaxLength().Should().Be(180);
    }

    [Fact]
    public void UserConfiguration_Name_HasMaxLength120()
    {
        IModel model = CreateModel();
        IProperty property = model.FindEntityType(typeof(User))!.FindProperty(nameof(User.Name))!;

        property.GetMaxLength().Should().Be(120);
    }

    [Fact]
    public void UserConfiguration_PasswordHash_HasMaxLength255()
    {
        IModel model = CreateModel();
        IProperty property = model.FindEntityType(typeof(User))!.FindProperty(nameof(User.PasswordHash))!;

        property.GetMaxLength().Should().Be(255);
    }

    [Fact]
    public void UserConfiguration_Email_HasUniqueIndex()
    {
        IModel model = CreateModel();
        IEntityType entityType = model.FindEntityType(typeof(User))!;

        bool hasUniqueEmailIndex = entityType.GetIndexes()
            .Any(index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(User.Email)));

        hasUniqueEmailIndex.Should().BeTrue();
    }

    // --- RefreshToken ---

    [Fact]
    public void RefreshTokenConfiguration_TokenHash_HasMaxLength128()
    {
        IModel model = CreateModel();
        IProperty property = model.FindEntityType(typeof(RefreshToken))!.FindProperty(nameof(RefreshToken.TokenHash))!;

        property.GetMaxLength().Should().Be(128);
    }

    [Fact]
    public void RefreshTokenConfiguration_TokenHash_HasUniqueIndex()
    {
        IModel model = CreateModel();
        IEntityType entityType = model.FindEntityType(typeof(RefreshToken))!;

        bool hasUniqueIndex = entityType.GetIndexes()
            .Any(index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(RefreshToken.TokenHash)));

        hasUniqueIndex.Should().BeTrue();
    }

    [Fact]
    public void RefreshTokenConfiguration_HasCompoundIndexOnUserIdAndIsRevoked()
    {
        IModel model = CreateModel();
        IEntityType entityType = model.FindEntityType(typeof(RefreshToken))!;

        bool hasCompoundIndex = entityType.GetIndexes()
            .Any(index => index.Properties.Count == 2
                && index.Properties.Any(p => p.Name == nameof(RefreshToken.UserId))
                && index.Properties.Any(p => p.Name == nameof(RefreshToken.IsRevoked)));

        hasCompoundIndex.Should().BeTrue();
    }

    [Fact]
    public void RefreshTokenConfiguration_User_HasCascadeDelete()
    {
        IModel model = CreateModel();
        IEntityType entityType = model.FindEntityType(typeof(RefreshToken))!;

        IForeignKey foreignKey = entityType.GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(User));

        foreignKey.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }

    // --- ClientDetail ---

    [Fact]
    public void ClientDetailConfiguration_CPF_HasMaxLength14()
    {
        IModel model = CreateModel();
        IProperty property = model.FindEntityType(typeof(ClientDetail))!.FindProperty(nameof(ClientDetail.CPF))!;

        property.GetMaxLength().Should().Be(14);
    }

    [Fact]
    public void ClientDetailConfiguration_Phone_HasMaxLength20()
    {
        IModel model = CreateModel();
        IProperty property = model.FindEntityType(typeof(ClientDetail))!.FindProperty(nameof(ClientDetail.Phone))!;

        property.GetMaxLength().Should().Be(20);
    }

    [Fact]
    public void ClientDetailConfiguration_Notes_HasMaxLength1000()
    {
        IModel model = CreateModel();
        IProperty property = model.FindEntityType(typeof(ClientDetail))!.FindProperty(nameof(ClientDetail.Notes))!;

        property.GetMaxLength().Should().Be(1000);
    }

    [Fact]
    public void ClientDetailConfiguration_CPF_HasUniqueIndex()
    {
        IModel model = CreateModel();
        IEntityType entityType = model.FindEntityType(typeof(ClientDetail))!;

        bool hasUniqueIndex = entityType.GetIndexes()
            .Any(index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(ClientDetail.CPF)));

        hasUniqueIndex.Should().BeTrue();
    }

    [Fact]
    public void ClientDetailConfiguration_UserId_HasUniqueIndex()
    {
        IModel model = CreateModel();
        IEntityType entityType = model.FindEntityType(typeof(ClientDetail))!;

        bool hasUniqueIndex = entityType.GetIndexes()
            .Any(index => index.IsUnique && index.Properties.Any(p => p.Name == nameof(ClientDetail.UserId)));

        hasUniqueIndex.Should().BeTrue();
    }

    [Fact]
    public void ClientDetailConfiguration_User_HasCascadeDelete()
    {
        IModel model = CreateModel();
        IEntityType entityType = model.FindEntityType(typeof(ClientDetail))!;

        IForeignKey foreignKey = entityType.GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(User));

        foreignKey.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }
}
