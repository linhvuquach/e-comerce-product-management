using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductManagement.Application.Products.Queries.GetProducts;

namespace ProductManagement.Infrastructure.BackgroundServices;

internal sealed partial class ProductCacheWarmingService(
    IServiceScopeFactory scopeFactory,
    ILogger<ProductCacheWarmingService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        LogWarmingStarted(logger);
        try
        {
            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            // Warm the default first-page product list
            await mediator.Send(new GetProductsQuery(Page: 1, PageSize: 20), cancellationToken);

            LogWarmingCompleted(logger);
        }
        catch (Exception ex)
        {
            LogWarmingFailed(logger, ex);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    [LoggerMessage(Level = LogLevel.Information, Message = "Product cache warming started.")]
    private static partial void LogWarmingStarted(ILogger logger);

    [LoggerMessage(Level = LogLevel.Information, Message = "Product cache warming completed.")]
    private static partial void LogWarmingCompleted(ILogger logger);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Product cache warming failed. API will serve cold cache on startup.")]
    private static partial void LogWarmingFailed(ILogger logger, Exception ex);
}
