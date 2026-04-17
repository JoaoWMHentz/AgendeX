namespace AgendeX.Domain.Entities;

public class ClientDetail
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string CPF { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string? Notes { get; set; }

    public User User { get; set; } = null!;
}
