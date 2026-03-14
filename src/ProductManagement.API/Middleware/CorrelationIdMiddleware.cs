using Serilog.Context;

namespace ProductManagement.API.Middleware;

internal sealed class CorrelationIdMiddleware(RequestDelegate next)
{
    private const string _headerName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[_headerName].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Response.Headers[_headerName] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
