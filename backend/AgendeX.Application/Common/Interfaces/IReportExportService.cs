using AgendeX.Application.Features.Reports;

namespace AgendeX.Application.Common.Interfaces;

public interface IReportExportService
{
    byte[] BuildCsv(IReadOnlyList<ReportRowDto> rows);
    byte[] BuildXlsx(IReadOnlyList<ReportAggregateDto> aggregates, IReadOnlyList<ReportRowDto> rows);
}