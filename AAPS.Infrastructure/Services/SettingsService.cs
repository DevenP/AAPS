using AAPS.Application.Abstractions.Services;
using AAPS.Application.Common.Paging;
using AAPS.Application.DTO;
using AAPS.Domain.Entities;
using AAPS.Infrastructure.Common.Extensions;
using AAPS.Infrastructure.Data.Scaffolded;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly IDbContextFactory<AppDbContext> _factory;

    public SettingsService(IDbContextFactory<AppDbContext> factory) => _factory = factory;

    public async Task<PagedResult<SettingDTO>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var query = db.SystemSettings
            .AsNoTracking()
            .Select(s => new SettingDTO { Key = s.SettingKey, Value = s.SettingValue, Description = s.Description });

        return await query.ToPagedResultAsync(request, ct);
    }

    public async Task<SettingDTO?> GetByKeyAsync(string key, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.SystemSettings
            .AsNoTracking()
            .Where(s => s.SettingKey == key)
            .Select(s => new SettingDTO { Key = s.SettingKey, Value = s.SettingValue, Description = s.Description })
            .FirstOrDefaultAsync(ct);
    }

    public async Task<string?> GetAsync(string key, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        return await db.SystemSettings
            .AsNoTracking()
            .Where(s => s.SettingKey == key)
            .Select(s => s.SettingValue)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<int> GetIntAsync(string key, int fallback, CancellationToken ct = default)
    {
        var raw = await GetAsync(key, ct);
        return int.TryParse(raw, out var value) ? value : fallback;
    }

    public async Task SetAsync(string key, string? value, CancellationToken ct = default)
    {
        await using var db = _factory.CreateDbContext();
        var entity = await db.SystemSettings.FirstOrDefaultAsync(s => s.SettingKey == key, ct);
        if (entity is null)
        {
            entity = new SystemSetting { SettingKey = key, SettingValue = value };
            db.SystemSettings.Add(entity);
        }
        else
        {
            entity.SettingValue = value;
        }
        await db.SaveChangesAsync(ct);
    }
}
