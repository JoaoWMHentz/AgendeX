using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;

namespace AgendeX.Application.Features.Reports;

internal static class ReportProcessing
{
    internal static readonly HashSet<string> AllowedSortColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        "clientName",
        "agentName",
        "appointmentDate",
        "appointmentTime",
        "serviceType",
        "status",
        "createdAt",
        "confirmedAt",
        "canceledAt",
        "reason"
    };

    internal static ReportFilterParameters SanitizeForAdmin(ReportFilterParameters filters)
    {
        return new ReportFilterParameters(
            NormalizeGuidList(filters.ClientIds),
            NormalizeGuidList(filters.AgentIds),
            filters.From,
            filters.To,
            NormalizeIntList(filters.ServiceTypeIds),
            NormalizeStatusList(filters.Statuses),
            filters.ReportType,
            NormalizeSortBy(filters.SortBy),
            filters.SortDirection);
    }

    internal static ReportFilterParameters SanitizeForAgent(
        ReportFilterParameters filters,
        Guid agentId,
        IReadOnlyCollection<Guid> allowedClientIds)
    {
        IReadOnlyList<Guid>? requestedClients = NormalizeGuidList(filters.ClientIds);

        IReadOnlyList<Guid>? effectiveClientIds = requestedClients is null
            ? allowedClientIds.Distinct().ToList()
            : requestedClients.Where(allowedClientIds.Contains).Distinct().ToList();

        return new ReportFilterParameters(
            effectiveClientIds,
            [agentId],
            filters.From,
            filters.To,
            NormalizeIntList(filters.ServiceTypeIds),
            NormalizeStatusList(filters.Statuses),
            filters.ReportType,
            NormalizeSortBy(filters.SortBy),
            filters.SortDirection);
    }

    internal static IReadOnlyList<ReportRowDto> MapRows(IReadOnlyList<Appointment> appointments)
    {
        return appointments
            .Select(a => new ReportRowDto(
                a.Id,
                a.Client.Name,
                a.Agent.Name,
                a.Date,
                a.Time,
                a.ServiceType.Description,
                a.Status,
                a.CreatedAt,
                a.ConfirmedAt,
                a.CanceledAt,
                a.RejectionReason))
            .ToList()
            .AsReadOnly();
    }

    internal static IReadOnlyList<ReportRowDto> SortRows(
        IReadOnlyList<ReportRowDto> rows,
        string? sortBy,
        ReportSortDirection direction)
    {
        string column = NormalizeSortBy(sortBy);
        bool descending = direction == ReportSortDirection.Desc;

        IOrderedEnumerable<ReportRowDto> ordered = column.ToLowerInvariant() switch
        {
            "clientname" => descending
                ? rows.OrderByDescending(r => r.ClientName).ThenByDescending(r => r.AppointmentDate).ThenByDescending(r => r.AppointmentTime)
                : rows.OrderBy(r => r.ClientName).ThenBy(r => r.AppointmentDate).ThenBy(r => r.AppointmentTime),
            "agentname" => descending
                ? rows.OrderByDescending(r => r.AgentName).ThenByDescending(r => r.AppointmentDate).ThenByDescending(r => r.AppointmentTime)
                : rows.OrderBy(r => r.AgentName).ThenBy(r => r.AppointmentDate).ThenBy(r => r.AppointmentTime),
            "appointmentdate" => descending
                ? rows.OrderByDescending(r => r.AppointmentDate).ThenByDescending(r => r.AppointmentTime)
                : rows.OrderBy(r => r.AppointmentDate).ThenBy(r => r.AppointmentTime),
            "appointmenttime" => descending
                ? rows.OrderByDescending(r => r.AppointmentTime).ThenByDescending(r => r.AppointmentDate)
                : rows.OrderBy(r => r.AppointmentTime).ThenBy(r => r.AppointmentDate),
            "servicetype" => descending
                ? rows.OrderByDescending(r => r.ServiceType).ThenByDescending(r => r.AppointmentDate).ThenByDescending(r => r.AppointmentTime)
                : rows.OrderBy(r => r.ServiceType).ThenBy(r => r.AppointmentDate).ThenBy(r => r.AppointmentTime),
            "status" => descending
                ? rows.OrderByDescending(r => r.Status).ThenByDescending(r => r.AppointmentDate).ThenByDescending(r => r.AppointmentTime)
                : rows.OrderBy(r => r.Status).ThenBy(r => r.AppointmentDate).ThenBy(r => r.AppointmentTime),
            "createdat" => descending
                ? rows.OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.AppointmentDate).ThenByDescending(r => r.AppointmentTime)
                : rows.OrderBy(r => r.CreatedAt).ThenBy(r => r.AppointmentDate).ThenBy(r => r.AppointmentTime),
            "confirmedat" => descending
                ? rows.OrderByDescending(r => r.ConfirmedAt).ThenByDescending(r => r.AppointmentDate).ThenByDescending(r => r.AppointmentTime)
                : rows.OrderBy(r => r.ConfirmedAt).ThenBy(r => r.AppointmentDate).ThenBy(r => r.AppointmentTime),
            "canceledat" => descending
                ? rows.OrderByDescending(r => r.CanceledAt).ThenByDescending(r => r.AppointmentDate).ThenByDescending(r => r.AppointmentTime)
                : rows.OrderBy(r => r.CanceledAt).ThenBy(r => r.AppointmentDate).ThenBy(r => r.AppointmentTime),
            "reason" => descending
                ? rows.OrderByDescending(r => r.RejectionOrCancellationReason).ThenByDescending(r => r.AppointmentDate).ThenByDescending(r => r.AppointmentTime)
                : rows.OrderBy(r => r.RejectionOrCancellationReason).ThenBy(r => r.AppointmentDate).ThenBy(r => r.AppointmentTime),
            _ => descending
                ? rows.OrderByDescending(r => r.AppointmentDate).ThenByDescending(r => r.AppointmentTime)
                : rows.OrderBy(r => r.AppointmentDate).ThenBy(r => r.AppointmentTime)
        };

        return ordered.ToList().AsReadOnly();
    }

    internal static IReadOnlyList<ReportAggregateDto> BuildAggregates(
        IReadOnlyList<ReportRowDto> rows,
        ReportType reportType)
    {
        return reportType switch
        {
            ReportType.TotalAppointmentsByAgent => rows
                .GroupBy(r => r.AgentName)
                .OrderByDescending(g => g.Count())
                .Select(g => new ReportAggregateDto(g.Key, g.Count()))
                .ToList()
                .AsReadOnly(),

            ReportType.TotalAppointmentsByClient => rows
                .GroupBy(r => r.ClientName)
                .OrderByDescending(g => g.Count())
                .Select(g => new ReportAggregateDto(g.Key, g.Count()))
                .ToList()
                .AsReadOnly(),

            ReportType.AppointmentsByStatus => rows
                .GroupBy(r => r.Status.ToString())
                .OrderByDescending(g => g.Count())
                .Select(g => new ReportAggregateDto(g.Key, g.Count()))
                .ToList()
                .AsReadOnly(),

            ReportType.CompletedVsCanceledRate => BuildCompletionVsCancellationRate(rows),

            ReportType.AppointmentsByServiceType => rows
                .GroupBy(r => r.ServiceType)
                .OrderByDescending(g => g.Count())
                .Select(g => new ReportAggregateDto(g.Key, g.Count()))
                .ToList()
                .AsReadOnly(),

            _ => []
        };
    }

    internal static string BuildFileName(string extension)
    {
        string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        return $"reports_{timestamp}.{extension}";
    }

    private static IReadOnlyList<ReportAggregateDto> BuildCompletionVsCancellationRate(IReadOnlyList<ReportRowDto> rows)
    {
        int completed = rows.Count(r => r.Status == AppointmentStatus.Completed);
        int canceled = rows.Count(r => r.Status == AppointmentStatus.Canceled);
        int denominator = completed + canceled;

        decimal completedRate = denominator == 0 ? 0 : Math.Round((decimal)completed / denominator * 100, 2);
        decimal canceledRate = denominator == 0 ? 0 : Math.Round((decimal)canceled / denominator * 100, 2);

        return new List<ReportAggregateDto>
        {
            new("CompletedRatePercent", completedRate),
            new("CanceledRatePercent", canceledRate),
            new("CompletedTotal", completed),
            new("CanceledTotal", canceled)
        }.AsReadOnly();
    }

    private static IReadOnlyList<Guid>? NormalizeGuidList(IReadOnlyList<Guid>? values)
    {
        if (values is null || values.Count == 0) return null;

        List<Guid> normalized = values
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        return normalized.Count == 0 ? null : normalized;
    }

    private static IReadOnlyList<int>? NormalizeIntList(IReadOnlyList<int>? values)
    {
        if (values is null || values.Count == 0) return null;

        List<int> normalized = values
            .Where(v => v > 0)
            .Distinct()
            .ToList();

        return normalized.Count == 0 ? null : normalized;
    }

    private static IReadOnlyList<AppointmentStatus>? NormalizeStatusList(IReadOnlyList<AppointmentStatus>? values)
    {
        if (values is null || values.Count == 0) return null;

        List<AppointmentStatus> normalized = values
            .Distinct()
            .ToList();

        return normalized.Count == 0 ? null : normalized;
    }

    private static string NormalizeSortBy(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy)) return "appointmentDate";

        return AllowedSortColumns.Contains(sortBy)
            ? sortBy
            : "appointmentDate";
    }
}
