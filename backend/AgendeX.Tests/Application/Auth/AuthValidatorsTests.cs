using AgendeX.Application.Features.Auth.Commands.Login;
using AgendeX.Application.Features.Auth.Commands.Logout;
using AgendeX.Application.Features.Auth.Commands.RefreshToken;
using FluentAssertions;

namespace AgendeX.Tests.Application.Auth;

public sealed class AuthValidatorsTests
{
    [Fact]
    public void LoginCommandValidator_InvalidEmail_ReturnsValidationError()
    {
        LoginCommandValidator validator = new();

        var result = validator.Validate(new LoginCommand("email-invalido", "123456"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Email");
    }

    [Fact]
    public void LoginCommandValidator_ShortPassword_ReturnsValidationError()
    {
        LoginCommandValidator validator = new();

        var result = validator.Validate(new LoginCommand("user@email.com", "123"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "Password");
    }

    [Fact]
    public void LoginCommandValidator_ValidInput_ReturnsSuccess()
    {
        LoginCommandValidator validator = new();

        var result = validator.Validate(new LoginCommand("user@email.com", "123456"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RefreshTokenCommandValidator_EmptyToken_ReturnsValidationError()
    {
        RefreshTokenCommandValidator validator = new();

        var result = validator.Validate(new RefreshTokenCommand(string.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "RefreshToken");
    }

    [Fact]
    public void RefreshTokenCommandValidator_ValidInput_ReturnsSuccess()
    {
        RefreshTokenCommandValidator validator = new();

        var result = validator.Validate(new RefreshTokenCommand("token-valido"));

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LogoutCommandValidator_EmptyToken_ReturnsValidationError()
    {
        LogoutCommandValidator validator = new();

        var result = validator.Validate(new LogoutCommand(string.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == "RefreshToken");
    }

    [Fact]
    public void LogoutCommandValidator_ValidInput_ReturnsSuccess()
    {
        LogoutCommandValidator validator = new();

        var result = validator.Validate(new LogoutCommand("token-valido"));

        result.IsValid.Should().BeTrue();
    }
}
