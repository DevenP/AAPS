using AAPS.Application.Common.Extensions;
using AAPS.Application.Common.Paging;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

namespace AAPS.Infrastructure.Common.Extensions
{
    public static class QueryableExtensions
    {
        // ── Reflection cache — populated once per DTO type, reused on every request ──
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _browseableStringProps = new();
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _allProps = new();

        // Cache the EF.Functions.Like MethodInfo — looked up once at startup
        private static readonly MethodInfo _likeMethod =
            typeof(DbFunctionsExtensions).GetMethod("Like",
                new[] { typeof(DbFunctions), typeof(string), typeof(string) })!;

        private static PropertyInfo[] GetBrowsableStringProps(Type type) =>
            _browseableStringProps.GetOrAdd(type, t =>
                t.GetProperties()
                 .Where(p => p.PropertyType == typeof(string) && p.IsBrowsable())
                 .ToArray());

        private static PropertyInfo[] GetAllProps(Type type) =>
            _allProps.GetOrAdd(type, t => t.GetProperties());

        // ── In-memory overload (for raw SQL / stored proc results) ───────────────
        // Use this when the source is already materialized (e.g. from SqlQueryRaw).
        // Applies search, column filters, sorting, and paging entirely in memory.
        public static Task<AAPS.Application.Common.Paging.PagedResult<T>> ToPagedResultAsync<T>(
            this IEnumerable<T> source,
            PagedRequest request,
            CancellationToken ct = default) where T : class
        {
            var query = source;

            if (!string.IsNullOrWhiteSpace(request.Search))
                query = ApplySearchInMemory(query, request.Search.Trim());

            if (request.ColumnFilters != null && request.ColumnFilters.Count > 0)
                query = ApplyColumnFiltersInMemory(query, request.ColumnFilters);

            if (!string.IsNullOrWhiteSpace(request.SortBy))
                query = ApplySortInMemory(query, request.SortBy, request.SortDir);

            var list = query.ToList();
            var totalCount = list.Count;

            // Export — return all items
            if (request.PageSize == -1)
                return Task.FromResult(new AAPS.Application.Common.Paging.PagedResult<T>(list, 1, totalCount, totalCount));

            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 25 : request.PageSize;

            var items = list
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(new AAPS.Application.Common.Paging.PagedResult<T>(items, page, pageSize, totalCount));
        }

        private static IEnumerable<T> ApplySearchInMemory<T>(IEnumerable<T> source, string term) where T : class
        {
            var props = GetBrowsableStringProps(typeof(T));

            return source.Where(item =>
                props.Any(p =>
                {
                    var val = p.GetValue(item) as string;
                    return val != null && val.Contains(term, StringComparison.OrdinalIgnoreCase);
                }));
        }

        private static IEnumerable<T> ApplyColumnFiltersInMemory<T>(IEnumerable<T> source, Dictionary<string, string> filters) where T : class
        {
            var properties = GetAllProps(typeof(T));

            foreach (var filter in filters)
            {
                if (filter.Key == "Global") continue;
                if (string.IsNullOrWhiteSpace(filter.Value)) continue;

                var cleanKey = filter.Key.Replace("_From", "").Replace("_To", "");
                var prop = properties.FirstOrDefault(p => p.Name.Equals(cleanKey, StringComparison.OrdinalIgnoreCase));
                if (prop == null) continue;

                var type = prop.PropertyType;

                if (type == typeof(bool) || type == typeof(bool?))
                {
                    if (bool.TryParse(filter.Value, out var boolVal))
                        source = source.Where(item =>
                        {
                            var v = prop.GetValue(item);
                            return v != null && (bool)v == boolVal;
                        });
                }
                else if (type == typeof(DateTime) || type == typeof(DateTime?))
                {
                    var val = filter.Value.ToLower();
                    if (val == "null")
                        source = source.Where(item => prop.GetValue(item) == null);
                    else if (val == "notnull")
                        source = source.Where(item => prop.GetValue(item) != null);
                    else if (filter.Key.EndsWith("_From") && DateTime.TryParse(filter.Value, out var from))
                        source = source.Where(item => { var v = prop.GetValue(item) as DateTime?; return v.HasValue && v.Value >= from; });
                    else if (filter.Key.EndsWith("_To") && DateTime.TryParse(filter.Value, out var to))
                    {
                        var endOfDay = to.Date.AddDays(1).AddTicks(-1);
                        source = source.Where(item => { var v = prop.GetValue(item) as DateTime?; return v.HasValue && v.Value <= endOfDay; });
                    }
                }
                else if (type == typeof(string))
                {
                    var val = filter.Value.ToLower();
                    if (val == "null")
                        source = source.Where(item => string.IsNullOrWhiteSpace(prop.GetValue(item) as string));
                    else if (val == "notnull")
                        source = source.Where(item => !string.IsNullOrWhiteSpace(prop.GetValue(item) as string));
                    else
                        source = source.Where(item => (prop.GetValue(item) as string ?? "").Contains(filter.Value, StringComparison.OrdinalIgnoreCase));
                }
                else if (IsNumericType(type))
                {
                    var val = filter.Value.ToLower();
                    if (val == "null")
                        source = source.Where(item => prop.GetValue(item) == null);
                    else if (val == "notnull")
                        source = source.Where(item => prop.GetValue(item) != null);
                    else if (filter.Key.EndsWith("_From") && decimal.TryParse(filter.Value, out var fromNum))
                        source = source.Where(item => { var v = prop.GetValue(item); return v != null && Convert.ToDecimal(v) >= fromNum; });
                    else if (filter.Key.EndsWith("_To") && decimal.TryParse(filter.Value, out var toNum))
                        source = source.Where(item => { var v = prop.GetValue(item); return v != null && Convert.ToDecimal(v) <= toNum; });
                    else if (decimal.TryParse(filter.Value, out var exact))
                        source = source.Where(item => { var v = prop.GetValue(item); return v != null && Convert.ToDecimal(v) == exact; });
                }
            }
            return source;
        }

        private static IEnumerable<T> ApplySortInMemory<T>(IEnumerable<T> source, string sortBy, string dir) where T : class
        {
            var prop = typeof(T).GetProperty(sortBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop == null) return source;

            var descending = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);
            return descending
                ? source.OrderByDescending(x => prop.GetValue(x))
                : source.OrderBy(x => prop.GetValue(x));
        }

        // ── Apply search + column filters without paging (used for aggregate queries) ──
        public static IQueryable<T> ApplyFilters<T>(this IQueryable<T> query, PagedRequest request, bool performSearch = true) where T : class
        {
            if (performSearch && !string.IsNullOrWhiteSpace(request.Search))
                query = ApplySearch(query, request.Search.Trim());
            if (request.ColumnFilters != null && request.ColumnFilters.Count > 0)
                query = ApplyColumnFilters(query, request.ColumnFilters);
            return query;
        }

        // ── EF Core / IQueryable overload ────────────────────────────────────────
        public static async Task<AAPS.Application.Common.Paging.PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            PagedRequest request,
            CancellationToken ct = default,
            bool performSearch = true) where T : class
        {
            if (performSearch && !string.IsNullOrWhiteSpace(request.Search))
                query = ApplySearch(query, request.Search.Trim());

            if (request.ColumnFilters != null && request.ColumnFilters.Count > 0)
                query = ApplyColumnFilters(query, request.ColumnFilters);

            // Export — return everything without a separate COUNT query
            if (request.PageSize == -1)
            {
                query = ApplySort(query, request.SortBy, request.SortDir);
                var allItems = await query.ToListAsync(ct);
                return new AAPS.Application.Common.Paging.PagedResult<T>(allItems, 1, allItems.Count, allItems.Count);
            }

            // Count before sorting — ORDER BY is meaningless for COUNT(*) and causes
            // EF Core to expand complex DTO projections into the sort key, breaking translation.
            var totalCount = await query.CountAsync(ct);

            // Sorting — apply after count for stable pagination
            query = ApplySort(query, request.SortBy, request.SortDir);

            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 25 : request.PageSize;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new AAPS.Application.Common.Paging.PagedResult<T>(items, page, pageSize, totalCount);
        }

        private static IQueryable<T> ApplyColumnFilters<T>(IQueryable<T> query, Dictionary<string, string> filters)
        {
            var properties = GetAllProps(typeof(T));

            foreach (var filter in filters)
            {
                if (filter.Key == "Global") continue;
                if (string.IsNullOrWhiteSpace(filter.Value)) continue;

                var cleanKey = filter.Key.Replace("_From", "").Replace("_To", "");
                var prop = properties.FirstOrDefault(p => p.Name.Equals(cleanKey, StringComparison.OrdinalIgnoreCase));
                if (prop == null) continue;

                var type = prop.PropertyType;

                if (type == typeof(bool) || type == typeof(bool?))
                {
                    if (bool.TryParse(filter.Value, out var boolValue))
                        query = query.Where($"{prop.Name} == @0", boolValue);
                }
                else if (type == typeof(DateTime) || type == typeof(DateTime?))
                {
                    var val = filter.Value.ToLower();
                    if (val == "null")
                        query = query.Where($"{prop.Name} == null");
                    else if (val == "notnull")
                        query = query.Where($"{prop.Name} != null");
                    else if (filter.Key.EndsWith("_From") && DateTime.TryParse(filter.Value, out var fromDate))
                        query = query.Where($"{prop.Name} != null && {prop.Name} >= @0", fromDate);
                    else if (filter.Key.EndsWith("_To") && DateTime.TryParse(filter.Value, out var toDate))
                    {
                        var endOfDay = toDate.Date.AddDays(1).AddTicks(-1);
                        query = query.Where($"{prop.Name} != null && {prop.Name} <= @0", endOfDay);
                    }
                }
                else if (type == typeof(string))
                {
                    var val = filter.Value.ToLower();
                    if (val == "null")
                        query = query.Where($"string.IsNullOrEmpty({prop.Name}) || {prop.Name}.Trim() == \"\"");
                    else if (val == "notnull")
                        query = query.Where($"!string.IsNullOrEmpty({prop.Name}) && {prop.Name}.Trim() != \"\"");
                    else
                        query = query.Where($"{prop.Name}.Contains(@0)", filter.Value);
                }
                else if (IsNumericType(type))
                {
                    var val = filter.Value.ToLower();
                    if (val == "null")
                        query = query.Where($"{prop.Name} == null");
                    else if (val == "notnull")
                        query = query.Where($"{prop.Name} != null");
                    else if (filter.Key.EndsWith("_From") && decimal.TryParse(filter.Value, out var fromNum))
                        query = query.Where($"{prop.Name} != null && {prop.Name} >= @0", fromNum);
                    else if (filter.Key.EndsWith("_To") && decimal.TryParse(filter.Value, out var toNum))
                        query = query.Where($"{prop.Name} != null && {prop.Name} <= @0", toNum);
                    else if (decimal.TryParse(filter.Value, out var exact))
                        query = query.Where($"{prop.Name} == @0", exact);
                }
            }
            return query;
        }

        private static IQueryable<T> ApplySearch<T>(IQueryable<T> query, string term)
        {
            var param = Expression.Parameter(typeof(T), "e");
            var stringProperties = GetBrowsableStringProps(typeof(T));

            Expression? filterBody = null;
            var pattern = Expression.Constant($"%{term}%");

            foreach (var prop in stringProperties)
            {
                try
                {
                    var member = Expression.Property(param, prop);
                    var likeCall = Expression.Call(
                        null, _likeMethod,
                        Expression.Property(null, typeof(EF), nameof(EF.Functions)),
                        member, pattern);
                    filterBody = filterBody == null ? likeCall : Expression.OrElse(filterBody, likeCall);
                }
                catch (Exception)
                {
                    // Property not translatable to SQL — skip it
                    continue;
                }
            }

            if (filterBody == null) return query;
            return query.Where(Expression.Lambda<Func<T, bool>>(filterBody, param));
        }

        private static IQueryable<T> ApplySort<T>(IQueryable<T> query, string? sortBy, string dir)
        {
            var hasId = typeof(T).GetProperty("Id") != null;
            var column = !string.IsNullOrWhiteSpace(sortBy) ? sortBy : (hasId ? "Id" : null);

            if (column == null) return query;

            var direction = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

            try
            {
                query = query.OrderBy($"{column} {direction}");

                // Secondary sort on Id for stable pagination across pages
                if (column != "Id" && hasId)
                    query = ((IOrderedQueryable<T>)query).ThenBy("Id asc");
            }
            catch (Exception)
            {
                // Column doesn't exist on the DTO — try Id fallback, otherwise leave unsorted
                if (hasId)
                    query = query.OrderBy("Id asc");
            }

            return query;
        }

        private static bool IsNumericType(Type type)
        {
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            return underlying == typeof(int)
                || underlying == typeof(long)
                || underlying == typeof(decimal)
                || underlying == typeof(double)
                || underlying == typeof(float)
                || underlying == typeof(short);
        }
    }
}
