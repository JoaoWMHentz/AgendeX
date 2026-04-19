using AgendeX.Application.Features.Reports;
using AgendeX.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgendeX.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = $"{Roles.Administrator},{Roles.Agent}")]
public sealed class ReportsController : ControllerBase
{
    private readonly ISender _sender;

    public ReportsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ReportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromQuery] ReportFilterRequest request, CancellationToken cancellationToken)
    {
        ReportFilterParameters filters = request.ToFilters();
        ReportResultDto report = await _sender.Send(new GetReportsQuery(filters), cancellationToken);

        return Ok(report);
    }

    [HttpGet("export/csv")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportCsv([FromQuery] ReportFilterRequest request, CancellationToken cancellationToken)
    {
        ReportFileDto file = await _sender.Send(new ExportReportsCsvQuery(request.ToFilters()), cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    [HttpGet("export/xlsx")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportXlsx([FromQuery] ReportFilterRequest request, CancellationToken cancellationToken)
    {
        ReportFileDto file = await _sender.Send(new ExportReportsXlsxQuery(request.ToFilters()), cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }

    public sealed record ReportFilterRequest(
        Guid[]? ClientIds,
        Guid[]? AgentIds,
        DateOnly? From,
        DateOnly? To,
        int[]? ServiceTypeIds,
        AppointmentStatus[]? Statuses,
        ReportType ReportType = ReportType.TotalAppointmentsByAgent,
        string? SortBy = null,
        ReportSortDirection SortDirection = ReportSortDirection.Desc)
    {
        public ReportFilterParameters ToFilters()
        {
            return new ReportFilterParameters(
                ClientIds,
                AgentIds,
                From,
                To,
                ServiceTypeIds,
                Statuses,
                ReportType,
                SortBy,
                SortDirection);
        }
    }
}
