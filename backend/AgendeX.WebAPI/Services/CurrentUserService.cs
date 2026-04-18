using AgendeX.Application.Common.Interfaces;
using AgendeX.Domain.Enums;
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
        _httpContextAccessor.HttpContext!.User;

    public Guid UserId =>
        Guid.Parse(User.FindFirst("sub")!.Value);

    public UserRole Role =>
        Enum.Parse<UserRole>(User.FindFirst(ClaimTypes.Role)!.Value);
}
