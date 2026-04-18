using AgendeX.Infrastructure.Services;
using FluentAssertions;

namespace AgendeX.Tests.Infrastructure.Auth;

public sealed class PasswordHasherTests
{
    [Fact]
    public void Hash_AndVerify_WithCorrectPassword_ReturnsTrue()
    {
        PasswordHasher passwordHasher = new();

        string hash = passwordHasher.Hash("Senha@123");
        bool isValid = passwordHasher.Verify("Senha@123", hash);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ReturnsFalse()
    {
        PasswordHasher passwordHasher = new();

        string hash = passwordHasher.Hash("Senha@123");
        bool isValid = passwordHasher.Verify("Senha@456", hash);

        isValid.Should().BeFalse();
    }
}
