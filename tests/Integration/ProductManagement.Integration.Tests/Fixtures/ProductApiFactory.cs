using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ProductManagement.Infrastructure.Persistence;
using Testcontainers.PostgreSql;
using Testcontainers.Redis;

namespace ProductManagement.Integration.Tests.Fixtures;

/// <summary>
/// Spins up a real PostgreSQL and Redis via Testcontainers, runs EF migrations,
/// and provides a configured <see cref="HttpClient"/> for endpoint tests.
/// Containers start once per test-class that uses <see cref="IClassFixture{ProductApiFactory}"/>.
/// </summary>
public sealed class ProductApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5432))
        .Build();

    private readonly RedisContainer _redis = new RedisBuilder()
        .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(6379))
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override connection strings to point at Testcontainers instances
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgres.GetConnectionString(),
                ["ConnectionStrings:Redis"] = _redis.GetConnectionString(),
                // Disable JWT auth — the fallback policy allows all requests
                ["Auth:Enabled"] = "false",
                // Keep rate-limit permissive so tests are not throttled
                ["RateLimit:WriteRequestsPerMinute"] = "10000",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove background cache-warming to keep tests fast and deterministic
            services.RemoveAll<IHostedService>();
        });

        builder.UseEnvironment("Testing");
    }

    public async Task InitializeAsync()
    {
        // Start containers first — ConfigureWebHost reads connection strings lazily
        await Task.WhenAll(_postgres.StartAsync(), _redis.StartAsync());

        // Run EF Core migrations directly (bypasses app DI) so we can suppress
        // PendingModelChangesWarning which EF 10 promotes to an error by default.
        var options = new DbContextOptionsBuilder<ProductManagementDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .UseSnakeCaseNamingConvention()
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning))
            .Options;

        await using var db = new ProductManagementDbContext(options);
        await db.Database.MigrateAsync();
    }

    public new async Task DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await _redis.DisposeAsync();
        await base.DisposeAsync();
    }
}
