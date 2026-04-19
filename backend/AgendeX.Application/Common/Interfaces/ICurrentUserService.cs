using AgendeX.Domain.Enums;

namespace AgendeX.Application.Common.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    UserRole Role { get; }
    bool IsAdmin { get; }
    bool IsAgent { get; }
    bool IsClient { get; }
}
