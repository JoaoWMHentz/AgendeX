namespace AgendeX.Application.Features.Reports;

public enum ReportType
{
    TotalAppointmentsByAgent,
    TotalAppointmentsByClient,
    AppointmentsByStatus,
    CompletedVsCanceledRate,
    AppointmentsByServiceType
}

public enum ReportSortDirection
{
    Asc,
    Desc
}
