using FluentValidation;
using AgendeX.Domain.Enums;

namespace AgendeX.Application.Features.Availability;

public sealed class CreateAvailabilityCommandValidator : AbstractValidator<CreateAvailabilityCommand>
{
    public CreateAvailabilityCommandValidator()
    {
        RuleFor(c => c.AgentId).NotEmpty();
        RuleFor(c => c.WeekDays)
            .NotEmpty()
            .WithMessage("At least one week day must be informed.");

        RuleFor(c => c.WeekDays)
            .Must(days => days.Distinct().Count() == days.Count)
            .WithMessage("Week days must not contain duplicates.");

        RuleForEach(c => c.WeekDays)
            .Must(weekDay => weekDay >= WeekDay.Monday && weekDay <= WeekDay.Friday)
            .WithMessage("Only Monday to Friday is allowed.");

        RuleFor(c => c.EndTime).GreaterThan(c => c.StartTime)
            .WithMessage("EndTime must be after StartTime.");

        RuleFor(c => c.SlotDurationMinutes)
            .Must(d => d is null or 30 or 60)
            .WithMessage("SlotDurationMinutes must be 30 or 60.");

        RuleFor(c => c)
            .Must(c => c.SlotDurationMinutes is null ||
                (int)(c.EndTime - c.StartTime).TotalMinutes % c.SlotDurationMinutes.Value == 0)
            .When(c => c.SlotDurationMinutes.HasValue && c.EndTime > c.StartTime)
            .WithMessage("The time range must be exactly divisible by the slot duration.");
    }
}
