namespace AgendeX.Domain.Entities;

public class ServiceType
{
    public int Id { get; private set; }
    public string Description { get; private set; } = string.Empty;

    protected ServiceType() { }

    public ServiceType(string description)
    {
        Description = description;
    }
}
