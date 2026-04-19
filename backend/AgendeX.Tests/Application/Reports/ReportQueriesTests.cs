using AgendeX.Application.Common.Interfaces;
using AgendeX.Application.Features.Reports;
using AgendeX.Domain.Entities;
using AgendeX.Domain.Enums;
using AgendeX.Domain.Interfaces;
using AgendeX.Tests.Application.Common;
using FluentAssertions;
using Moq;

namespace AgendeX.Tests.Application.Reports;

public sealed class ReportQueriesTests
{
    [Fact]
    public async Task GetReportsHandle_AdminRequest_ReturnsSortedRowsAndAggregates()
    {
        Mock<IReportRepository> repository = new();
        Mock<ICurrentUserService> currentUser = new();

        currentUser.SetupGet(c => c.IsAdmin).Returns(true);
        currentUser.SetupGet(c => c.IsAgent).Returns(false);

        Appointment appointmentA = EntityTestFactory.CreateAppointment(status: AppointmentStatus.Completed);
        EntityTestFactory.PopulateAppointmentNavigations(appointmentA, clientName: "Carlos", agentName: "Bruno");

        Appointment appointmentB = EntityTestFactory.CreateAppointment(status: AppointmentStatus.Canceled);
        EntityTestFactory.PopulateAppointmentNavigations(appointmentB, clientName: "Ana", agentName: "Bruno");

        repository
            .Setup(r => r.GetReportAppointmentsAsync(
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                It.IsAny<IReadOnlyCollection<int>?>(),
                It.IsAny<IReadOnlyCollection<AppointmentStatus>?>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([appointmentA, appointmentB]);

        GetReportsQueryHandler handler = new(repository.Object, currentUser.Object);

        ReportResultDto result = await handler.Handle(
            new GetReportsQuery(new ReportFilterParameters(
                null,
                null,
                null,
                null,
                null,
                null,
                ReportType.TotalAppointmentsByAgent,
                "clientName",
                ReportSortDirection.Asc)),
            CancellationToken.None);

        result.Rows.Should().HaveCount(2);
        result.Rows.First().ClientName.Should().Be("Ana");
        result.Aggregates.Should().ContainSingle(a => a.Label == "Bruno" && a.Value == 2);
    }

    [Fact]
    public async Task GetReportsHandle_AgentRequest_ForcesAgentAndClientScope()
    {
        Mock<IReportRepository> repository = new();
        Mock<ICurrentUserService> currentUser = new();

        Guid loggedAgentId = Guid.NewGuid();
        Guid allowedClient = Guid.NewGuid();
        Guid deniedClient = Guid.NewGuid();

        currentUser.SetupGet(c => c.IsAdmin).Returns(false);
        currentUser.SetupGet(c => c.IsAgent).Returns(true);
        currentUser.SetupGet(c => c.UserId).Returns(loggedAgentId);

        repository
            .Setup(r => r.GetAgentClientIdsAsync(loggedAgentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([allowedClient]);

        repository
            .Setup(r => r.GetReportAppointmentsAsync(
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                It.IsAny<IReadOnlyCollection<int>?>(),
                It.IsAny<IReadOnlyCollection<AppointmentStatus>?>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        GetReportsQueryHandler handler = new(repository.Object, currentUser.Object);

        await handler.Handle(
            new GetReportsQuery(new ReportFilterParameters(
                [allowedClient, deniedClient],
                [Guid.NewGuid()],
                null,
                null,
                null,
                null,
                ReportType.AppointmentsByStatus,
                null,
                ReportSortDirection.Desc)),
            CancellationToken.None);

        repository.Verify(r => r.GetReportAppointmentsAsync(
            It.Is<IReadOnlyCollection<Guid>?>(ids => ids != null && ids.Count == 1 && ids.Contains(allowedClient)),
            It.Is<IReadOnlyCollection<Guid>?>(ids => ids != null && ids.Count == 1 && ids.Contains(loggedAgentId)),
            null,
            null,
            null,
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExportCsvHandle_AdminRequest_ReturnsCsvFile()
    {
        Mock<IReportRepository> repository = new();
        Mock<ICurrentUserService> currentUser = new();
        Mock<IReportExportService> exportService = new();

        currentUser.SetupGet(c => c.IsAdmin).Returns(true);
        currentUser.SetupGet(c => c.IsAgent).Returns(false);

        Appointment appointment = EntityTestFactory.CreateAppointment();
        repository
            .Setup(r => r.GetReportAppointmentsAsync(
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                It.IsAny<IReadOnlyCollection<Guid>?>(),
                It.IsAny<IReadOnlyCollection<int>?>(),
                It.IsAny<IReadOnlyCollection<AppointmentStatus>?>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<DateOnly?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([appointment]);

        exportService
            .Setup(s => s.BuildCsv(It.IsAny<IReadOnlyList<ReportRowDto>>()))
            .Returns([1, 2, 3]);

        ExportReportsCsvQueryHandler handler = new(repository.Object, currentUser.Object, exportService.Object);

        ReportFileDto result = await handler.Handle(
            new ExportReportsCsvQuery(new ReportFilterParameters(
                null,
                null,
                null,
                null,
                null,
                null,
                ReportType.TotalAppointmentsByClient,
                null,
                ReportSortDirection.Desc)),
            CancellationToken.None);

        result.ContentType.Should().Be("text/csv");
        result.Content.Should().Equal([1, 2, 3]);
        result.FileName.Should().EndWith(".csv");
    }
}
