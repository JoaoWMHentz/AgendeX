using AgendeX.Application.Common.Interfaces;
using AgendeX.Domain.Interfaces;
using AgendeX.Infrastructure.Identity;
using AgendeX.Infrastructure.Persistence;
using AgendeX.Infrastructure.Persistence.Repositories;
using AgendeX.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgendeX.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? configuredConnectionString = configuration.GetConnectionString("Default");

        string connectionString = string.IsNullOrWhiteSpace(configuredConnectionString)
            ? BuildLocalPostgresConnectionString(configuration)
            : configuredConnectionString;

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<RsaKeyProvider>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IClientDetailRepository, ClientDetailRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IServiceTypeRepository, ServiceTypeRepository>();
        services.AddScoped<IAgentAvailabilityRepository, AgentAvailabilityRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IReportExportService, ReportExportService>();

        return services;
    }

    private static string BuildLocalPostgresConnectionString(IConfiguration configuration)
    {
        string host = configuration["POSTGRES_HOST"] ?? "localhost";
        string port = configuration["POSTGRES_PORT"] ?? "5432";
        string database = configuration["POSTGRES_DB"] ?? "agendex";
        string username = configuration["POSTGRES_USER"] ?? "agendex";
        string password = configuration["POSTGRES_PASSWORD"] ?? "agendex";

        return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
}
