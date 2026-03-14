using System.Globalization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Carter;
using Prometheus;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ProductManagement.API.Middleware;
using ProductManagement.API.Options;
using ProductManagement.Application;
using ProductManagement.Application.Common.Behaviours;
using ProductManagement.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// ── Serilog ─────────────────────────────────────────────────────────────────
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture));

// ── Settings ─────────────────────────────────────────────────────────────────
var corsSettings = builder.Configuration
    .GetSection(CorsSettings.Section)
    .Get<CorsSettings>() ?? new CorsSettings();

var rateLimitSettings = builder.Configuration
    .GetSection(RateLimitSettings.Section)
    .Get<RateLimitSettings>() ?? new RateLimitSettings();

var authSettings = builder.Configuration
    .GetSection(AuthSettings.Section)
    .Get<AuthSettings>() ?? new AuthSettings();

var otelSettings = builder.Configuration
    .GetSection(OpenTelemetrySettings.Section)
    .Get<OpenTelemetrySettings>() ?? new OpenTelemetrySettings();

// ── Infrastructure & Application ──────────────────────────────────────────────
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// ── API Versioning ────────────────────────────────────────────────────────────
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// ── OpenAPI ───────────────────────────────────────────────────────────────────
builder.Services.AddOpenApi();

// ── CORS ──────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsSettings.PolicyName, policy =>
        policy.WithOrigins(corsSettings.AllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("ETag", "X-Correlation-ID"));
});

// ── Carter ────────────────────────────────────────────────────────────────────
builder.Services.AddCarter();

// ── Problem Details + Global Exception Handler ────────────────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// ── Rate Limiting ─────────────────────────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(RateLimitSettings.WritePolicyName, limiter =>
    {
        limiter.PermitLimit = rateLimitSettings.WriteRequestsPerMinute;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = rateLimitSettings.QueueLimit;
    });
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// ── Health Checks ─────────────────────────────────────────────────────────────
var dbConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
var redisConnectionString = builder.Configuration.GetConnectionString("Redis")!;

builder.Services.AddHealthChecks()
    .AddNpgSql(dbConnectionString, name: "database", tags: ["ready"])
    .AddRedis(redisConnectionString, name: "redis", tags: ["ready"]);

// ── Authentication & Authorization ────────────────────────────────────────────
if (authSettings.Enabled)
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.Authority = authSettings.Authority;
            options.Audience = authSettings.Audience;
        });
    builder.Services.AddAuthorizationBuilder();
}
else
{
    // Auth scaffold is disabled — register a no-op scheme and allow all requests through
    builder.Services.AddAuthentication();
    var allowAll = new AuthorizationPolicyBuilder()
        .RequireAssertion(_ => true)
        .Build();
    builder.Services.AddAuthorizationBuilder()
        .SetDefaultPolicy(allowAll)
        .SetFallbackPolicy(allowAll);
}

// ── OpenTelemetry ─────────────────────────────────────────────────────────────
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(otelSettings.ServiceName))
    .WithTracing(tracing =>
    {
        tracing
            .AddSource(ApplicationActivitySource.Source.Name)
            .AddSource("Npgsql")
            .AddAspNetCoreInstrumentation()
            .AddRedisInstrumentation();

        if (!string.IsNullOrEmpty(otelSettings.OtlpEndpoint))
            tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otelSettings.OtlpEndpoint));

        if (otelSettings.ConsoleExporter)
            tracing.AddConsoleExporter();
    });

// ── Build ─────────────────────────────────────────────────────────────────────
var app = builder.Build();

// ── Middleware pipeline ───────────────────────────────────────────────────────
app.UseExceptionHandler();
app.UseMiddleware<CorrelationIdMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "Product Management API";
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();
app.UseCors(CorsSettings.PolicyName);
app.UseRateLimiter();
app.UseHttpMetrics();

app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = hc => hc.Tags.Contains("ready")
});

app.MapMetrics("/metrics");

app.Run();
