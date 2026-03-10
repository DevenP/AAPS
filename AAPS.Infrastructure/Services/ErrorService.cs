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

        // Log to application logger
        _logger.LogError(exception, "Error in {Context}: {Message}", context, message);

        // Notify subscribers (UI components can subscribe to show notifications)
        OnError?.Invoke(errorInfo);
    }

    public string? GetLastErrorMessage() => _lastError?.Message;

    public void ClearError() => _lastError = null;
}
