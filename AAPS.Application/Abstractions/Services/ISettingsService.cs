using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;

namespace AAPS.Application.Abstractions.Services;

/// <summary>
/// Reads and writes the app's key/value system settings so tunable values can be changed from
/// Configuration instead of a code release.
/// </summary>
public interface ISettingsService
{
    /// <summary>All settings as rows, for the Configuration table.</summary>
    Task<PagedResult<SettingDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default);

    /// <summary>One setting row by its key, or null when it doesn't exist.</summary>
    Task<SettingDTO?> GetByKeyAsync(string key, CancellationToken ct = default);

    /// <summary>The raw stored value for a key, or null when it isn't set.</summary>
    Task<string?> GetAsync(string key, CancellationToken ct = default);

    /// <summary>The stored value parsed as an int, or <paramref name="fallback"/> when missing or unparseable.</summary>
    Task<int> GetIntAsync(string key, int fallback, CancellationToken ct = default);

    /// <summary>Store (or clear) a value for a key, creating the row if needed.</summary>
    Task SetAsync(string key, string? value, CancellationToken ct = default);
}
