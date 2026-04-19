using AgendeX.Application.Common.Interfaces;
using AgendeX.Domain.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AgendeX.WebAPI.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User =>
        _httpContextAccessor.HttpContext?.User
        ?? throw new UnauthorizedAccessException("Authenticated user context is not available.");

    public Guid UserId =>
        GetRequiredGuidClaim(JwtRegisteredClaimNames.Sub, ClaimTypes.NameIdentifier);

    public UserRole Role =>
        GetRequiredEnumClaim<UserRole>(ClaimTypes.Role);

    public bool IsAdmin => Role == UserRole.Administrator;

    public bool IsAgent => Role == UserRole.Agent;

    public bool IsClient => Role == UserRole.Client;

    private Guid GetRequiredGuidClaim(params string[] claimTypes)
    {
        string claimValue = GetRequiredClaimValue(claimTypes);

        if (!Guid.TryParse(claimValue, out Guid userId))
        {
            throw new UnauthorizedAccessException("Authenticated user identifier is invalid.");
        }

        return userId;
    }

    private TEnum GetRequiredEnumClaim<TEnum>(params string[] claimTypes)
        where TEnum : struct, Enum
    {
        string claimValue = GetRequiredClaimValue(claimTypes);

        if (!Enum.TryParse(claimValue, true, out TEnum result))
        {
            throw new UnauthorizedAccessException($"Authenticated user role '{claimValue}' is invalid.");
        }

        return result;
    }

    private string GetRequiredClaimValue(params string[] claimTypes)
    {
        foreach (string claimType in claimTypes)
        {
            string? value = User.FindFirstValue(claimType);

            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        throw new UnauthorizedAccessException(
            $"Authenticated user is missing one of the required claims: {string.Join(", ", claimTypes)}.");
    }
}
