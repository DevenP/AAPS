using AAPS.Application.Common.Crud;
using AAPS.Application.VendorPortals;
using AAPS.Infrastructure.Data.Scaffolded;
using AAPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace AAPS.Infrastructure.VendorPortals;

public sealed class VendorPortalCrudService : IVendorPortalCrudService
{
    private readonly AppDbContext _db;

    public VendorPortalCrudService(AppDbContext db) => _db = db;

    public async Task<CrudResult<Dictionary<string, object?>>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var entity = await FindByIdAsync(id, ct);

        if (entity is null)
        {
            return new(CrudStatus.NotFound, Message: "VendorPortal not found.");
        } 

        return new(CrudStatus.Success, ToRow(entity));
    }

    public async Task<CrudResult<object>> CreateAsync(Dictionary<string, object?> values, CancellationToken ct = default)
    {
        var entity = new VendorPortal();

        ApplyValues(entity, values, includeKeys: true);

        _db.VendorPortals.Add(entity);
        await _db.SaveChangesAsync(ct);

        // best-effort: return PK value (works for common "Id" patterns)
        var pk = GuessKeyValue(entity);
        return new(CrudStatus.Success, pk);
    }

    public async Task<SaveResult> UpdateAsync(int id, Dictionary<string, object?> values, string? rowVersionBase64, CancellationToken ct = default)
    {
        var entity = await FindByIdAsync(id, ct);
        if (entity is null) return new(CrudStatus.NotFound, "VendorPortal not found.");

        // Concurrency: if your table has a rowversion/timestamp column scaffolded as byte[]
        if (!TryApplyRowVersionOriginalValue(entity, rowVersionBase64))
        {
            // If the table doesn't have a rowversion column, we can’t enforce optimistic concurrency
            // (still safe if you accept last-write-wins)
        }

        ApplyValues(entity, values, includeKeys: false);

        try
        {
            await _db.SaveChangesAsync(ct);
            return new(CrudStatus.Success);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(CrudStatus.Conflict, "This record was modified by someone else. Refresh and try again.");
        }
    }

    public async Task<SaveResult> DeleteAsync(int id, string? rowVersionBase64, CancellationToken ct = default)
    {
        var entity = await FindByIdAsync(id, ct);
        if (entity is null) return new(CrudStatus.NotFound, "VendorPortal not found.");

        TryApplyRowVersionOriginalValue(entity, rowVersionBase64);

        _db.VendorPortals.Remove(entity);

        try
        {
            await _db.SaveChangesAsync(ct);
            return new(CrudStatus.Success);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new(CrudStatus.Conflict, "This record was modified by someone else. Refresh and try again.");
        }
    }

    // ----------------- helpers -----------------

    private async Task<VendorPortal?> FindByIdAsync(object id, CancellationToken ct)
    {
        // If VendorPortal has a single key, EF can FindAsync it.
        // This will throw if the key shape doesn't match (composite keys).
        try
        {
            return await _db.VendorPortals.FindAsync([id], ct);
        }
        catch
        {
            // fallback: try common key names
            var keyProp = typeof(VendorPortal).GetProperties()
                .FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase)
                                  || p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));

            if (keyProp is null) return null;

            // build query: where EF.Property<object>(e, keyName) == id
            return await _db.VendorPortals
                .FirstOrDefaultAsync(e => EF.Property<object>(e, keyProp.Name)!.Equals(id), ct);
        }
    }

    private static void ApplyValues(VendorPortal entity, Dictionary<string, object?> values, bool includeKeys)
    {
        var props = typeof(VendorPortal).GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var (key, val) in values)
        {
            var prop = props.FirstOrDefault(p => string.Equals(p.Name, key, StringComparison.OrdinalIgnoreCase));
            if (prop is null) continue;

            if (!includeKeys && IsKeyLike(prop)) continue;
            if (!IsSimpleType(prop.PropertyType)) continue;
            if (!prop.CanWrite) continue;

            object? converted = ConvertTo(prop.PropertyType, val);
            prop.SetValue(entity, converted);
        }
    }

    private static bool IsKeyLike(PropertyInfo p) 
    {
        return string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase)
                || p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSimpleType(Type t)
    {
        t = Nullable.GetUnderlyingType(t) ?? t;
        return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal)
               || t == typeof(DateTime) || t == typeof(DateTimeOffset) || t == typeof(Guid) || t == typeof(byte[]);
    }

    private static object? ConvertTo(Type targetType, object? value)
    {
        if (value is null) return null;

        var t = Nullable.GetUnderlyingType(targetType) ?? targetType;

        if (t == typeof(byte[]) && value is string s)
        {
            // allow setting rowversion/bytes from base64
            try { return Convert.FromBase64String(s); } catch { return null; }
        }

        if (t.IsEnum)
        {
            if (value is string es) return Enum.Parse(t, es, ignoreCase: true);
            return Enum.ToObject(t, value);
        }

        if (t == typeof(Guid))
        {
            if (value is string gs) return Guid.Parse(gs);
        }

        return Convert.ChangeType(value, t);
    }

    private bool TryApplyRowVersionOriginalValue(VendorPortal entity, string? rowVersionBase64)
    {
        if (string.IsNullOrWhiteSpace(rowVersionBase64)) return false;

        var rvProp = typeof(VendorPortal).GetProperties()
            .FirstOrDefault(p => p.PropertyType == typeof(byte[])
                              && (p.Name.Equals("RowVersion", StringComparison.OrdinalIgnoreCase)
                               || p.Name.Equals("Timestamp", StringComparison.OrdinalIgnoreCase)));

        if (rvProp is null) return false;

        byte[] rv;
        try { rv = Convert.FromBase64String(rowVersionBase64); }
        catch { return false; }

        _db.Entry(entity).Property(rvProp.Name).OriginalValue = rv;
        return true;
    }

    private static Dictionary<string, object?> ToRow(VendorPortal entity)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in typeof(VendorPortal).GetProperties())
        {
            if (!IsSimpleType(p.PropertyType)) continue;
            var val = p.GetValue(entity);

            // If rowversion is byte[], return it as base64 so the client can send it back
            if (val is byte[] bytes) dict[p.Name] = Convert.ToBase64String(bytes);
            else dict[p.Name] = val;
        }
        return dict;
    }

    private static object GuessKeyValue(VendorPortal entity)
    {
        var prop = typeof(VendorPortal).GetProperties()
            .FirstOrDefault(p => string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase))
            ?? typeof(VendorPortal).GetProperties().FirstOrDefault(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase));

        return prop?.GetValue(entity) ?? "(unknown key)";
    }
}
