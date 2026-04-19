using AgendeX.Domain.Enums;

namespace AgendeX.Application.Features.Reports;

public sealed record ReportFilterParameters(
    IReadOnlyList<Guid>? ClientIds,
    IReadOnlyList<Guid>? AgentIds,
    DateOnly? From,
    DateOnly? To,
    IReadOnlyList<int>? ServiceTypeIds,
    IReadOnlyList<AppointmentStatus>? Statuses,
    ReportType ReportType,
    string? SortBy,
    ReportSortDirection SortDirection = ReportSortDirection.Desc);
