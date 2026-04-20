using AgendeX.Application;
using AgendeX.Infrastructure;
using AgendeX.Infrastructure.Identity;
using AgendeX.Infrastructure.Persistence;
using AgendeX.WebAPI.Middlewares;
using AgendeX.WebAPI.Services;
using AgendeX.WebAPI.Serialization;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

var builder = WebApplication.CreateBuilder(args);

ApplyDevelopmentJwtFallback(builder);

string? apiPortValue = builder.Configuration["Api:Port"];
if (!string.IsNullOrWhiteSpace(apiPortValue))
{
    if (!int.TryParse(apiPortValue, out int apiPort) || apiPort is < 1 or > 65535)
    {
        throw new InvalidOperationException("Configuration 'Api:Port' must be a valid TCP port between 1 and 65535.");
    }

    builder.WebHost.UseUrls($"http://+:{apiPort}");
}

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new TimeOnlyMinutesJsonConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    OpenApiSecurityScheme jwtSecurityScheme = new()
    {
        Name = "Authorization",
        Description = "Enter a valid JWT bearer token.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = JwtBearerDefaults.AuthenticationScheme.ToLowerInvariant(),
        BearerFormat = "JWT"
    };

    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtSecurityScheme);

    options.DocumentFilter<AllowAnonymousOperationFilter>();
    options.OperationFilter<SwaggerExamplesOperationFilter>();

    options.MapType<TimeOnly>(() => new OpenApiSchema
    {
        Type = JsonSchemaType.String,
        Format = "time",
        Example = JsonValue.Create("09:00")
    });

    options.MapType<DateOnly>(() => new OpenApiSchema
    {
        Type = JsonSchemaType.String,
        Format = "date",
        Example = JsonValue.Create("2026-04-25")
    });
});

builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitPolicies>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AgendeX.Application.Common.Interfaces.ICurrentUserService, AgendeX.WebAPI.Services.CurrentUserService>();
builder.Services.AddScoped<AdminSeedService>();
builder.Services.AddOptions<AdminSeedOptions>()
    .Bind(builder.Configuration.GetSection(AdminSeedOptions.SectionName));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();

builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<RsaKeyProvider, IOptions<JwtOptions>>((options, rsaKeyProvider, jwtOptions) =>
    {
        JwtOptions jwtOptionsValue = jwtOptions.Value;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptionsValue.Issuer,
            ValidAudience = jwtOptionsValue.Audience,
            IssuerSigningKey = rsaKeyProvider.PublicKey,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();

    AdminSeedService adminSeedService = scope.ServiceProvider.GetRequiredService<AdminSeedService>();
    await adminSeedService.SeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();
app.UseIpRateLimiting();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors("Frontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static void ApplyDevelopmentJwtFallback(WebApplicationBuilder builder)
{
    if (!builder.Environment.IsDevelopment())
    {
        return;
    }

    string? jwtIssuer = builder.Configuration["Jwt:Issuer"];
    string? jwtAudience = builder.Configuration["Jwt:Audience"];

    if (!string.IsNullOrWhiteSpace(jwtIssuer) && !string.IsNullOrWhiteSpace(jwtAudience))
    {
        return;
    }

    builder.Configuration["Jwt:Issuer"] = builder.Configuration["JWT_ISSUER"] ?? "AgendeX";
    builder.Configuration["Jwt:Audience"] = builder.Configuration["JWT_AUDIENCE"] ?? "AgendeX.Clients";
    builder.Configuration["Jwt:AccessTokenMinutes"] = builder.Configuration["JWT_ACCESS_TOKEN_MINUTES"] ?? "15";
    builder.Configuration["Jwt:RefreshTokenDays"] = builder.Configuration["JWT_REFRESH_TOKEN_DAYS"] ?? "7";
}
