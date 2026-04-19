using AgendeX.Domain.Enums;
using AgendeX.WebAPI.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AgendeX.Tests.Infrastructure.Auth;

public sealed class CurrentUserServiceTests
{
    [Fact]
    public void UserId_And_Role_ReturnValuesFromAuthenticatedUser()
    {
        Guid userId = Guid.NewGuid();
        DefaultHttpContext httpContext = new();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.Role, UserRole.Agent.ToString())
        ], authenticationType: "Bearer"));

        CurrentUserService currentUserService = new(new HttpContextAccessor { HttpContext = httpContext });

        currentUserService.UserId.Should().Be(userId);
        currentUserService.Role.Should().Be(UserRole.Agent);
        currentUserService.IsAdmin.Should().BeFalse();
        currentUserService.IsAgent.Should().BeTrue();
        currentUserService.IsClient.Should().BeFalse();
    }

    [Fact]
    public void UserId_WhenContextIsMissing_ThrowsUnauthorizedAccessException()
    {
        CurrentUserService currentUserService = new(new HttpContextAccessor());

        Action action = () => _ = currentUserService.UserId;

        action.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("Authenticated user context is not available.");
    }
}