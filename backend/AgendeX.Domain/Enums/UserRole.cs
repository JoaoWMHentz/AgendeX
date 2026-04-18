namespace AgendeX.Domain.Enums;

public enum UserRole
{
    Administrator,
    Agent,
    Client
}

public static class Roles
{
    public const string Administrator = nameof(UserRole.Administrator);
    public const string Agent = nameof(UserRole.Agent);
    public const string Client = nameof(UserRole.Client);
}
