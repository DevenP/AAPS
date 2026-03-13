using AAPS.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace AAPS.Web.Middleware;

/// <summary>
/// Global exception handling middleware for the application.
/// </summary>
public class GlobalExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly RequestDelegate _next;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (OperationCanceledException)
        {
            // Request was cancelled (e.g. MudTable debounce cancelling in-flight queries).
            // This is normal — do not treat as an error.
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(httpContext, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        var isApiRequest = context.Request.Path.StartsWithSegments("/vendorportals")
            || context.Request.Path.StartsWithSegments("/files/download")
            || context.Request.Path.StartsWithSegments("/files/preview")
            || context.Request.Headers["Accept"].Any(h => h!.Contains("application/json"));

        if (!isApiRequest)
        {
            // Re-execute through Blazor's error page instead
            context.Response.Redirect("/error");
            return;
        }

        context.Response.ContentType = "application/json";
        var (statusCode, message) = GetResponseDetails(exception);
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsJsonAsync(new
        {
            success = false,
            message,
            timestamp = DateTime.UtcNow
        });
    }

    private static (int statusCode, string message) GetResponseDetails(Exception exception)
    {
        return exception switch
        {
            KeyNotFoundException => (404, "The requested resource was not found"),
            ArgumentException => (400, "Invalid argument provided"),
            UnauthorizedAccessException => (401, "You are not authorized to perform this action"),
            InvalidOperationException => (409, "Invalid operation - conflict with current state"),
            _ => (500, "An unexpected error occurred. Please try again later")
        };
    }
}