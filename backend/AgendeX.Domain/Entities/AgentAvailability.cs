using AgendeX.Domain.Enums;

namespace AgendeX.Domain.Entities;

public class AgentAvailability
{
    public Guid Id { get; private set; }
    public Guid AgentId { get; private set; }
    public WeekDay WeekDay { get; private set; }
    public TimeOnly StartTime { get; private set; }
    public TimeOnly EndTime { get; private set; }
    public bool IsActive { get; private set; }

    public User Agent { get; private set; } = null!;

    protected AgentAvailability() { }

    public AgentAvailability(Guid agentId, WeekDay weekDay, TimeOnly startTime, TimeOnly endTime)
    {
        Id = Guid.NewGuid();
        AgentId = agentId;
        WeekDay = weekDay;
        StartTime = startTime;
        EndTime = endTime;
        IsActive = true;
    }

    public void Update(TimeOnly startTime, TimeOnly endTime)
    {
        StartTime = startTime;
        EndTime = endTime;
    }

    public void Deactivate() => IsActive = false;
}
