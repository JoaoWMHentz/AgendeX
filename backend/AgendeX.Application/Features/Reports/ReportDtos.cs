using AgendeX.Domain.Enums;

namespace AgendeX.Application.Features.Reports;

public sealed record ReportAggregateDto(string Label, decimal Value);

public sealed record ReportFileDto(string FileName, string ContentType, byte[] Content);

public sealed record ReportResultDto(
    ReportType ReportType,
    IReadOnlyList<ReportAggregateDto> Aggregates,
    IReadOnlyList<ReportRowDto> Rows);

public sealed record ReportRowDto(
    Guid AppointmentId,
    string ClientName,
    string AgentName,
    DateOnly AppointmentDate,
    TimeOnly AppointmentTime,
    string ServiceType,
    AppointmentStatus Status,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? CanceledAt,
    string? RejectionOrCancellationReason);
