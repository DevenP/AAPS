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
        catch (Exception exception)
        {
            await HandleExceptionAsync(httpContext, exception);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.LogError(exception, "An unhandled exception occurred");

        // Resolve scoped service from the request's service provider
        var errorService = context.RequestServices.GetRequiredService<IErrorService>();
        errorService.LogError(exception, "An error occurred processing your request", "Request Processing");

        context.Response.ContentType = "application/json";
        var (statusCode, message) = GetResponseDetails(exception);
        context.Response.StatusCode = statusCode;

        var response = new
        {
            success = false,
            message = message,
            timestamp = DateTime.UtcNow
        };

        return context.Response.WriteAsJsonAsync(response);
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

