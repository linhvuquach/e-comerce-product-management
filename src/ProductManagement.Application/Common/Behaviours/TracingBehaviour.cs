using System.Diagnostics;
using MediatR;

namespace ProductManagement.Application.Common.Behaviours;

internal sealed class TracingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        using var activity = ApplicationActivitySource.Source.StartActivity(
            $"MediatR/{requestName}",
            ActivityKind.Internal);

        activity?.SetTag("mediator.request", requestName);

        try
        {
            var response = await next(cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("exception.type", ex.GetType().FullName);
            activity?.SetTag("exception.message", ex.Message);
            throw;
        }
    }
}

public static class ApplicationActivitySource
{
    public static readonly ActivitySource Source = new("ProductManagement.Application");
}
