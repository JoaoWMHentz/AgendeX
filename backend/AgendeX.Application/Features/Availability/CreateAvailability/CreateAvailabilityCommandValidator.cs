using FluentValidation;

namespace AgendeX.Application.Features.Availability;

public sealed class CreateAvailabilityCommandValidator : AbstractValidator<CreateAvailabilityCommand>
{
    public CreateAvailabilityCommandValidator()
    {
        RuleFor(c => c.AgentId).NotEmpty();
        RuleFor(c => c.WeekDay).IsInEnum();
        RuleFor(c => c.EndTime).GreaterThan(c => c.StartTime)
            .WithMessage("EndTime must be after StartTime.");
    }
}
