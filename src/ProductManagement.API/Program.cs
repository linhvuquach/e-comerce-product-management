using Asp.Versioning;
using Carter;
using ProductManagement.API.Middleware;
using ProductManagement.API.Options;
using ProductManagement.Application;
using ProductManagement.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var corsSettings = builder.Configuration
    .GetSection(CorsSettings.Section)
    .Get<CorsSettings>() ?? new CorsSettings();

// Infrastructure & Application
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// OpenAPI
builder.Services.AddOpenApi();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsSettings.PolicyName, policy =>
        policy.WithOrigins(corsSettings.AllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("ETag"));
});

// Carter
builder.Services.AddCarter();

// Problem Details + Global Exception Handler
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();

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

app.MapCarter();
app.MapHealthChecks("/health");

app.Run();
