using FluentValidation;

namespace AgendeX.Application.Features.Reports;

public sealed class ExportReportsXlsxQueryValidator : AbstractValidator<ExportReportsXlsxQuery>
{
    public ExportReportsXlsxQueryValidator()
    {
        RuleFor(x => x.Filters.From)
            .LessThanOrEqualTo(x => x.Filters.To)
            .When(x => x.Filters.From.HasValue && x.Filters.To.HasValue)
            .WithMessage("From date must be less than or equal to To date.");

        RuleFor(x => x.Filters.SortBy)
            .Must(sortBy => string.IsNullOrWhiteSpace(sortBy) || ReportProcessing.AllowedSortColumns.Contains(sortBy))
            .WithMessage("SortBy is invalid.");
    }
}
