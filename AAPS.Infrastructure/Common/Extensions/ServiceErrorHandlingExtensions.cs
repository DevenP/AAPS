using AAPS.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace AAPS.Infrastructure.Common.Extensions;

/// <summary>
/// Extensions for wrapping service calls with error handling.
/// </summary>
public static class ServiceErrorHandlingExtensions
{
    /// <summary>
    /// Wraps an async service call with error handling and logging.
    /// </summary>
    public static async Task<T?> ExecuteSafelyAsync<T>(
        this IErrorService errorService,
        Func<Task<T>> operation,
        string context,
        T? defaultValue = default)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            errorService.LogError(ex, $"Error in {context}", context);
            return defaultValue;
        }
    }

    /// <summary>
    /// Wraps a void async service call with error handling and logging.
    /// </summary>
    public static async Task ExecuteSafelyAsync(
        this IErrorService errorService,
        Func<Task> operation,
        string context)
    {
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            errorService.LogError(ex, $"Error in {context}", context);
        }
    }

    /// <summary>
    /// Wraps a synchronous service call with error handling and logging.
    /// </summary>
    public static T? ExecuteSafely<T>(
        this IErrorService errorService,
        Func<T> operation,
        string context,
        T? defaultValue = default)
    {
        try
        {
            return operation();
        }
        catch (Exception ex)
        {
            errorService.LogError(ex, $"Error in {context}", context);
            return defaultValue;
        }
    }
}
