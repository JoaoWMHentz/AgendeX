using AgendeX.Domain.Enums;

namespace AgendeX.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    protected User() { }

    public User(string name, string email, string passwordHash, UserRole role)
    {
        Id = Guid.NewGuid();
        Name = name;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public ClientDetail? ClientDetail { get; private set; }

    public void Update(string name, UserRole? role = null, bool? isActive = null)
    {
        Name = name;
        if (role.HasValue) Role = role.Value;
        if (isActive.HasValue)
        {
            if (isActive.Value) Activate();
            else Deactivate();
        }
    }

    public void Deactivate() => IsActive = false;

    public void Activate() => IsActive = true;
}
