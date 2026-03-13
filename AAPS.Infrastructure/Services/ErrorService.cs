using AAPS.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace AAPS.Infrastructure.Services;

/// <summary>
/// Implementation of error service for handling and displaying errors.
/// </summary>
public class ErrorService : IErrorService
{
    private readonly ILogger<ErrorService> _logger;
    private ErrorInfo? _lastError;
    private Exception? _unhandledException;

    public event Action<ErrorInfo>? OnError;

    public ErrorService(ILogger<ErrorService> logger)
    {
        _logger = logger;
    }

    public void LogError(Exception exception, string? userMessage = null, string? context = null)
    {
        var message = userMessage ?? exception.Message;
        var errorInfo = new ErrorInfo(
            Message: message,
            Context: context,
            StackTrace: exception.StackTrace,
            OccurredAt: DateTime.UtcNow);

        _lastError = errorInfo;

        _logger.LogError(exception, "Error in {Context}: {Message}", context, message);

        OnError?.Invoke(errorInfo);
    }

    public void StoreUnhandledException(Exception exception)
    {
        _unhandledException = exception;
        _logger.LogError(exception, "Unhandled exception caught by ErrorBoundary");
    }

    public Exception? GetUnhandledException() => _unhandledException;

    public string? GetLastErrorMessage() => _lastError?.Message;

    public void ClearError()
    {
        _lastError = null;
        _unhandledException = null;
    }
}