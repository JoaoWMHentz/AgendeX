using System.Globalization;
using System.Text;
using AgendeX.Application.Common.Interfaces;
using AgendeX.Application.Features.Reports;
using ClosedXML.Excel;

namespace AgendeX.Infrastructure.Services;

public sealed class ReportExportService : IReportExportService
{
    public byte[] BuildCsv(IReadOnlyList<ReportRowDto> rows)
    {
        StringBuilder builder = new();

        builder.AppendLine(string.Join(',',
            "ClientName",
            "AgentName",
            "AppointmentDate",
            "AppointmentTime",
            "ServiceType",
            "Status",
            "CreatedAt",
            "ConfirmedAt",
            "CanceledAt",
            "RejectionOrCancellationReason"));

        foreach (ReportRowDto row in rows)
        {
            builder.AppendLine(string.Join(',',
                EscapeCsv(row.ClientName),
                EscapeCsv(row.AgentName),
                EscapeCsv(row.AppointmentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                EscapeCsv(row.AppointmentTime.ToString("HH:mm", CultureInfo.InvariantCulture)),
                EscapeCsv(row.ServiceType),
                EscapeCsv(row.Status.ToString()),
                EscapeCsv(row.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)),
                EscapeCsv(row.ConfirmedAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty),
                EscapeCsv(row.CanceledAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty),
                EscapeCsv(row.RejectionOrCancellationReason ?? string.Empty)));
        }

        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    public byte[] BuildXlsx(IReadOnlyList<ReportAggregateDto> aggregates, IReadOnlyList<ReportRowDto> rows)
    {
        using XLWorkbook workbook = new();

        BuildSummarySheet(workbook, aggregates);
        BuildDataSheet(workbook, rows);

        using MemoryStream stream = new();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void BuildSummarySheet(XLWorkbook workbook, IReadOnlyList<ReportAggregateDto> aggregates)
    {
        IXLWorksheet sheet = workbook.Worksheets.Add("Resumo");

        sheet.Cell(1, 1).Value = "Métrica";
        sheet.Cell(1, 2).Value = "Valor";
        sheet.Row(1).Style.Font.Bold = true;

        for (int i = 0; i < aggregates.Count; i++)
        {
            ReportAggregateDto agg = aggregates[i];
            int line = i + 2;
            sheet.Cell(line, 1).Value = agg.Label;
            sheet.Cell(line, 2).Value = agg.Value;
        }

        sheet.Columns().AdjustToContents();
    }

    private static void BuildDataSheet(XLWorkbook workbook, IReadOnlyList<ReportRowDto> rows)
    {
        IXLWorksheet sheet = workbook.Worksheets.Add("Dados");

        string[] headers =
        [
            "ClientName",
            "AgentName",
            "AppointmentDate",
            "AppointmentTime",
            "ServiceType",
            "Status",
            "CreatedAt",
            "ConfirmedAt",
            "CanceledAt",
            "RejectionOrCancellationReason"
        ];

        for (int i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
            sheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            ReportRowDto row = rows[i];
            int line = i + 2;

            sheet.Cell(line, 1).Value = row.ClientName;
            sheet.Cell(line, 2).Value = row.AgentName;
            sheet.Cell(line, 3).Value = row.AppointmentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            sheet.Cell(line, 4).Value = row.AppointmentTime.ToString("HH:mm", CultureInfo.InvariantCulture);
            sheet.Cell(line, 5).Value = row.ServiceType;
            sheet.Cell(line, 6).Value = row.Status.ToString();
            sheet.Cell(line, 7).Value = row.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            sheet.Cell(line, 8).Value = row.ConfirmedAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty;
            sheet.Cell(line, 9).Value = row.CanceledAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty;
            sheet.Cell(line, 10).Value = row.RejectionOrCancellationReason ?? string.Empty;
        }

        sheet.Columns().AdjustToContents();
    }

    private static string EscapeCsv(string value)
    {
        bool mustQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        if (!mustQuote) return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
