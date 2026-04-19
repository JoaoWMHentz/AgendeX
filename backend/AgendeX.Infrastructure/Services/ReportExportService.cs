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

    public byte[] BuildXlsx(IReadOnlyList<ReportRowDto> rows)
    {
        using XLWorkbook workbook = new();
        IXLWorksheet worksheet = workbook.Worksheets.Add("Reports");

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
            worksheet.Cell(1, i + 1).Value = headers[i];
            worksheet.Cell(1, i + 1).Style.Font.Bold = true;
        }

        for (int i = 0; i < rows.Count; i++)
        {
            ReportRowDto row = rows[i];
            int line = i + 2;

            worksheet.Cell(line, 1).Value = row.ClientName;
            worksheet.Cell(line, 2).Value = row.AgentName;
            worksheet.Cell(line, 3).Value = row.AppointmentDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            worksheet.Cell(line, 4).Value = row.AppointmentTime.ToString("HH:mm", CultureInfo.InvariantCulture);
            worksheet.Cell(line, 5).Value = row.ServiceType;
            worksheet.Cell(line, 6).Value = row.Status.ToString();
            worksheet.Cell(line, 7).Value = row.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            worksheet.Cell(line, 8).Value = row.ConfirmedAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty;
            worksheet.Cell(line, 9).Value = row.CanceledAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? string.Empty;
            worksheet.Cell(line, 10).Value = row.RejectionOrCancellationReason ?? string.Empty;
        }

        worksheet.Columns().AdjustToContents();

        using MemoryStream stream = new();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static string EscapeCsv(string value)
    {
        bool mustQuote = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        if (!mustQuote) return value;

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
