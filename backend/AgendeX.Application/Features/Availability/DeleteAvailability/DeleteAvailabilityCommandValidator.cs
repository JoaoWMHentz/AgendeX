using FluentValidation;

namespace AgendeX.Application.Features.Availability;

public sealed class DeleteAvailabilityCommandValidator : AbstractValidator<DeleteAvailabilityCommand>
{
    public DeleteAvailabilityCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
    }
}
