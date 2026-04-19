using AgendeX.Application.Common.Interfaces;
using AgendeX.Domain.Interfaces;
using MediatR;

namespace AgendeX.Application.Features.Reports;

public sealed record GetReportsQuery(ReportFilterParameters Filters)
    : IRequest<ReportResultDto>;


public sealed class GetReportsQueryHandler : IRequestHandler<GetReportsQuery, ReportResultDto>
{
    private readonly IReportRepository _repository;
    private readonly ICurrentUserService _currentUser;

    public GetReportsQueryHandler(IReportRepository repository, ICurrentUserService currentUser)
    {
        _repository = repository;
        _currentUser = currentUser;
    }

    public async Task<ReportResultDto> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAdmin && !_currentUser.IsAgent)
            throw new UnauthorizedAccessException("Only Administrator and Agent can access reports.");

        ReportFilterParameters filters = _currentUser.IsAdmin
            ? global::AgendeX.Application.Features.Reports.ReportProcessing.SanitizeForAdmin(request.Filters)
            : await BuildAgentScopedFiltersAsync(request.Filters, cancellationToken);

        if (filters.ClientIds is { Count: 0 })
            return new ReportResultDto(filters.ReportType, [], []);

        IReadOnlyList<Domain.Entities.Appointment> appointments = await _repository.GetReportAppointmentsAsync(
            filters.ClientIds,
            filters.AgentIds,
            filters.ServiceTypeIds,
            filters.Statuses,
            filters.From,
            filters.To,
            cancellationToken);

        IReadOnlyList<ReportRowDto> rows = global::AgendeX.Application.Features.Reports.ReportProcessing.MapRows(appointments);
        IReadOnlyList<ReportRowDto> sortedRows = global::AgendeX.Application.Features.Reports.ReportProcessing.SortRows(rows, filters.SortBy, filters.SortDirection);
        IReadOnlyList<ReportAggregateDto> aggregates = global::AgendeX.Application.Features.Reports.ReportProcessing.BuildAggregates(sortedRows, filters.ReportType);

        return new ReportResultDto(filters.ReportType, aggregates, sortedRows);
    }

    private async Task<ReportFilterParameters> BuildAgentScopedFiltersAsync(
        ReportFilterParameters filters,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Guid> allowedClientIds = await _repository.GetAgentClientIdsAsync(_currentUser.UserId, cancellationToken);

        return global::AgendeX.Application.Features.Reports.ReportProcessing.SanitizeForAgent(filters, _currentUser.UserId, allowedClientIds);
    }
}