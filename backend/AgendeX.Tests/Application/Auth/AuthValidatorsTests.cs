using AgendeX.Application.Features.Auth;
using FluentAssertions;

namespace AgendeX.Tests.Application.Auth;

public sealed class AuthValidatorsTests
{
    [Fact]
    public void LoginCommandValidator_InvalidEmailAndPassword_ShouldBeInvalid()
    {
        LoginCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(new LoginCommand(string.Empty, "123"));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Fact]
    public void RefreshTokenCommandValidator_EmptyToken_ShouldBeInvalid()
    {
        RefreshTokenCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(new RefreshTokenCommand(string.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RefreshToken");
    }

    [Fact]
    public void LogoutCommandValidator_EmptyToken_ShouldBeInvalid()
    {
        LogoutCommandValidator validator = new();

        FluentValidation.Results.ValidationResult result = validator.Validate(new LogoutCommand(string.Empty));

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "RefreshToken");
    }
}