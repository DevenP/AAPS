using System.Linq.Expressions;
using AAPS.Application.Common.Paging;
using AAPS.Application.VendorPortals;
using AAPS.Infrastructure.Data.Scaffolded;
using AAPS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Infrastructure.VendorPortals;

public sealed class VendorPortalQueryService : IVendorPortalQueryService
{
    private readonly AppDbContext _db;

    public VendorPortalQueryService(AppDbContext db) 
    {
        _db = db;
    } 

    public async Task<PagedResult<Dictionary<string, object?>>> GetAsync(
        PagedRequest request,
        CancellationToken ct = default)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 200 ? 25 : request.PageSize;

        IQueryable<VendorPortal> query = _db.VendorPortals.AsNoTracking();

        // Global search across ALL string columns (simple + useful)
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = ApplyStringSearch(query, request.Search.Trim());
        }

        // Sorting by column name (defaults to first property if none provided)
        query = ApplySorting(query, request.SortBy, request.SortDir);

        var total = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        // Convert entity -> row dictionary (keeps Application layer clean)
        var rows = items.Select(ToRow).ToList();

        return new PagedResult<Dictionary<string, object?>>(rows, page, pageSize, total);
    }

    private static Dictionary<string, object?> ToRow(VendorPortal entity)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var props = typeof(VendorPortal).GetProperties();

        foreach (var p in props)
        {
            // avoid navigation collections making huge payloads
            if (!IsSimpleType(p.PropertyType)) continue;

            dict[p.Name] = p.GetValue(entity);
        }

        return dict;
    }

    private static bool IsSimpleType(Type t)
    {
        t = Nullable.GetUnderlyingType(t) ?? t;

        return t.IsPrimitive
               || t.IsEnum
               || t == typeof(string)
               || t == typeof(decimal)
               || t == typeof(DateTime)
               || t == typeof(DateTimeOffset)
               || t == typeof(Guid);
    }

    private static IQueryable<VendorPortal> ApplySorting(
        IQueryable<VendorPortal> q,
        string? sortBy,
        string sortDir)
    {
        // pick default sort column if user didn’t send one
        var prop = typeof(VendorPortal).GetProperties()
            .FirstOrDefault(p => IsSimpleType(p.PropertyType));

        var col = string.IsNullOrWhiteSpace(sortBy) ? prop?.Name : sortBy;

        if (string.IsNullOrWhiteSpace(col))
            return q;

        var desc = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase);

        // EF.Property lets us sort by string column name
        return desc
            ? q.OrderByDescending(e => EF.Property<object>(e, col))
            : q.OrderBy(e => EF.Property<object>(e, col));
    }

    private static IQueryable<VendorPortal> ApplyStringSearch(
        IQueryable<VendorPortal> q,
        string term)
    {
        // Build: WHERE (col1 LIKE %term% OR col2 LIKE %term% OR ...)
        var param = Expression.Parameter(typeof(VendorPortal), "e");

        Expression? body = null;
        var likeMethod = typeof(DbFunctionsExtensions).GetMethod(
            nameof(DbFunctionsExtensions.Like),
            new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        var functions = Expression.Property(null, typeof(EF), nameof(EF.Functions));
        var pattern = Expression.Constant($"%{term}%");

        foreach (var p in typeof(VendorPortal).GetProperties().Where(p => p.PropertyType == typeof(string)))
        {
            var member = Expression.Property(param, p);
            var likeCall = Expression.Call(likeMethod!, functions, member, pattern);

            body = body is null ? likeCall : Expression.OrElse(body, likeCall);
        }

        if (body is null) return q; // no string columns

        var lambda = Expression.Lambda<Func<VendorPortal, bool>>(body, param);
        return q.Where(lambda);
    }
}
