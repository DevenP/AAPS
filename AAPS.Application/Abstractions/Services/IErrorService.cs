namespace AAPS.Application.Abstractions.Services;

/// <summary>
/// Service for handling and displaying application errors.
/// </summary>
public interface IErrorService
{
    /// <summary>
    /// Logs an exception and notifies listeners.
    /// </summary>
    void LogError(Exception exception, string? userMessage = null, string? context = null);

    /// <summary>
    /// Gets the last error message.
    /// </summary>
    string? GetLastErrorMessage();

    /// <summary>
    /// Clears the current error.
    /// </summary>
    void ClearError();

    /// <summary>
    /// Event raised when an error occurs.
    /// </summary>
    event Action<ErrorInfo>? OnError;
}

/// <summary>
/// Information about an error occurrence.
/// </summary>
public record ErrorInfo(
    string Message,
    string? Context,
    string? StackTrace,
    DateTime OccurredAt);
