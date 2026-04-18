using FluentValidation;

namespace AgendeX.Application.Features.Availability;

public sealed class UpdateAvailabilityCommandValidator : AbstractValidator<UpdateAvailabilityCommand>
{
    public UpdateAvailabilityCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.EndTime).GreaterThan(c => c.StartTime)
            .WithMessage("EndTime must be after StartTime.");
    }
}
