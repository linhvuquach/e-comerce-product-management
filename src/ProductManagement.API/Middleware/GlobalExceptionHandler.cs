using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Domain.Exceptions;

namespace ProductManagement.API.Middleware;

internal sealed partial class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, errors) = MapException(exception);

        LogUnhandledException(logger, exception, title);

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.io/{statusCode}",
            Instance = httpContext.Request.Path
        };

        if (errors is not null)
            problemDetails.Extensions["errors"] = errors;

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, object? Errors) MapException(Exception exception) =>
        exception switch
        {
            ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                ve.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray())),

            ProductNotFoundException or CategoryNotFoundException => (
                StatusCodes.Status404NotFound,
                exception.Message,
                null),

            InvalidProductStatusTransitionException => (
                StatusCodes.Status422UnprocessableEntity,
                exception.Message,
                null),

            DuplicateSkuException => (
                StatusCodes.Status409Conflict,
                exception.Message,
                null),

            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "The product has been modified by another session. Reload and retry.",
                null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                null),
        };

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception: {Title}")]
    private static partial void LogUnhandledException(ILogger logger, Exception ex, string title);
}
