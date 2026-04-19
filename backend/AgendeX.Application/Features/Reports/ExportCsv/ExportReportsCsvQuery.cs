using AgendeX.Application.Common.Interfaces;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Reports;

public sealed record ExportReportsCsvQuery(ReportFilterParameters Filters)
    : IRequest<ReportFileDto>;


public sealed class ExportReportsCsvQueryHandler : IRequestHandler<ExportReportsCsvQuery, ReportFileDto>
{
    private readonly IReportRepository _repository;
    private readonly ICurrentUserService _currentUser;
    private readonly IReportExportService _reportExportService;

    public ExportReportsCsvQueryHandler(
        IReportRepository repository,
        ICurrentUserService currentUser,
        IReportExportService reportExportService)
    {
        _repository = repository;
        _currentUser = currentUser;
        _reportExportService = reportExportService;
    }

    public async Task<ReportFileDto> Handle(ExportReportsCsvQuery request, CancellationToken cancellationToken)
    {
        IReadOnlyList<ReportRowDto> rows = await GetRowsAsync(request.Filters, cancellationToken);
        byte[] content = _reportExportService.BuildCsv(rows);

        return new ReportFileDto(
            ReportProcessing.BuildFileName("csv"),
            "text/csv",
            content);
    }

    private async Task<IReadOnlyList<ReportRowDto>> GetRowsAsync(
        ReportFilterParameters filters,
        CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin && !_currentUser.IsAgent)
            throw new UnauthorizedAccessException("Only Administrator and Agent can access reports.");

        ReportFilterParameters effectiveFilters;
        if (_currentUser.IsAdmin)
        {
            effectiveFilters = ReportProcessing.SanitizeForAdmin(filters);
        }
        else
        {
            IReadOnlyList<Guid> allowedClientIds = await _repository.GetAgentClientIdsAsync(_currentUser.UserId, cancellationToken);
            effectiveFilters = ReportProcessing.SanitizeForAgent(filters, _currentUser.UserId, allowedClientIds);
        }

        if (effectiveFilters.ClientIds is { Count: 0 })
            return [];

        IReadOnlyList<Domain.Entities.Appointment> appointments = await _repository.GetReportAppointmentsAsync(
            effectiveFilters.ClientIds,
            effectiveFilters.AgentIds,
            effectiveFilters.ServiceTypeIds,
            effectiveFilters.Statuses,
            effectiveFilters.From,
            effectiveFilters.To,
            cancellationToken);

        IReadOnlyList<ReportRowDto> rows = ReportProcessing.MapRows(appointments);
        return ReportProcessing.SortRows(rows, effectiveFilters.SortBy, effectiveFilters.SortDirection);
    }
}
