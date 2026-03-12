using AAPS.Application.Common.Paging;
using AAPS.Application.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Common.Extensions
{
    public static class QueryableExtensions
    {
        // ── In-memory overload (for raw SQL / stored proc results) ───────────────
        // Use this when the source is already materialized (e.g. from SqlQueryRaw).
        // Applies search, column filters, sorting, and paging entirely in memory.
        public static Task<Application.Common.Paging.PagedResult<T>> ToPagedResultAsync<T>(
            this IEnumerable<T> source,
            PagedRequest request,
            CancellationToken ct = default) where T : class
        {
            var query = source;

            // Global search
            if (!string.IsNullOrWhiteSpace(request.Search))
                query = ApplySearchInMemory(query, request.Search.Trim());

            // Column filters
            if (request.ColumnFilters != null && request.ColumnFilters.Any())
                query = ApplyColumnFiltersInMemory(query, request.ColumnFilters);

            // Sorting
            if (!string.IsNullOrWhiteSpace(request.SortBy))
                query = ApplySortInMemory(query, request.SortBy, request.SortDir);

            var list = query.ToList();
            var totalCount = list.Count;

            // Export
            if (request.PageSize == -1)
                return Task.FromResult(new Application.Common.Paging.PagedResult<T>(list, 1, totalCount, totalCount));

            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 25 : request.PageSize;

            var items = list
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Task.FromResult(new Application.Common.Paging.PagedResult<T>(items, page, pageSize, totalCount));
        }

        private static IEnumerable<T> ApplySearchInMemory<T>(IEnumerable<T> source, string term) where T : class
        {
            var props = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string) && p.IsBrowsable());

            return source.Where(item =>
                props.Any(p =>
                {
                    var val = p.GetValue(item) as string;
                    return val != null && val.Contains(term, StringComparison.OrdinalIgnoreCase);
                }));
        }

        private static IEnumerable<T> ApplyColumnFiltersInMemory<T>(IEnumerable<T> source, Dictionary<string, string> filters) where T : class
        {
            var properties = typeof(T).GetProperties();

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
                    if (filter.Key.EndsWith("_From") && DateTime.TryParse(filter.Value, out var from))
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
            }
            return source;
        }

        private static IEnumerable<T> ApplySortInMemory<T>(IEnumerable<T> source, string sortBy, string dir) where T : class
        {
            var prop = typeof(T).GetProperty(sortBy, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (prop == null) return source;

            var descending = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);
            return descending
                ? source.OrderByDescending(x => prop.GetValue(x))
                : source.OrderBy(x => prop.GetValue(x));
        }

        // ── EF Core / IQueryable overload ────────────────────────────────────────
        public static async Task<Application.Common.Paging.PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query,
            PagedRequest request,
            CancellationToken ct = default,
            bool performSearch = true) where T : class
        {
            // Global search (skip if the service handled it manually with performSearch: false)
            if (performSearch && !string.IsNullOrWhiteSpace(request.Search))
                query = ApplySearch(query, request.Search.Trim());

            // Column filters (date ranges, booleans, string contains/null checks)
            if (request.ColumnFilters != null && request.ColumnFilters.Any())
                query = ApplyColumnFilters(query, request.ColumnFilters);

            // Sorting
            if (!string.IsNullOrWhiteSpace(request.SortBy))
                query = ApplySort(query, request.SortBy, request.SortDir);

            // Export — return everything without a separate COUNT query
            if (request.PageSize == -1)
            {
                var allItems = await query.ToListAsync(ct);
                return new Application.Common.Paging.PagedResult<T>(allItems, 1, allItems.Count, allItems.Count);
            }

            var totalCount = await query.CountAsync(ct);

            var page = request.Page < 1 ? 1 : request.Page;
            var pageSize = request.PageSize <= 0 ? 25 : request.PageSize;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new Application.Common.Paging.PagedResult<T>(items, page, pageSize, totalCount);
        }

        private static IQueryable<T> ApplyColumnFilters<T>(IQueryable<T> query, Dictionary<string, string> filters)
        {
            var properties = typeof(T).GetProperties();

            foreach (var filter in filters)
            {
                if (filter.Key == "Global") continue; // handled separately as global search
                if (string.IsNullOrWhiteSpace(filter.Value)) continue;

                // Strip _From/_To suffix to get the real property name
                var cleanKey = filter.Key.Replace("_From", "").Replace("_To", "");
                var prop = properties.FirstOrDefault(p => p.Name.Equals(cleanKey, StringComparison.OrdinalIgnoreCase));

                if (prop == null) continue;

                var type = prop.PropertyType;

                // Handle BOOLEANS
                if (type == typeof(bool) || type == typeof(bool?))
                {
                    if (bool.TryParse(filter.Value, out var boolValue))
                        query = query.Where($"{prop.Name} == @0", boolValue);
                }
                // Handle DATE RANGES (From/To suffix)
                else if (type == typeof(DateTime) || type == typeof(DateTime?))
                {
                    if (filter.Key.EndsWith("_From") && DateTime.TryParse(filter.Value, out var fromDate))
                        query = query.Where($"{prop.Name} != null && {prop.Name} >= @0", fromDate);
                    else if (filter.Key.EndsWith("_To") && DateTime.TryParse(filter.Value, out var toDate))
                    {
                        var endOfDay = toDate.Date.AddDays(1).AddTicks(-1);
                        query = query.Where($"{prop.Name} != null && {prop.Name} <= @0", endOfDay);
                    }
                }
                // Handle STRINGS — "null", "notnull", or plain contains
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
            }
            return query;
        }

        private static IQueryable<T> ApplySearch<T>(IQueryable<T> query, string term)
        {
            var param = Expression.Parameter(typeof(T), "e");

            // Get string properties that are browsable
            var stringProperties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string) && p.IsBrowsable());

            Expression? filterBody = null;
            var pattern = Expression.Constant($"%{term}%");
            var likeMethod = typeof(DbFunctionsExtensions).GetMethod("Like", new[] { typeof(DbFunctions), typeof(string), typeof(string) });

            foreach (var prop in stringProperties)
            {
                try
                {
                    var member = Expression.Property(param, prop);
                    var likeCall = Expression.Call(null, likeMethod!, Expression.Property(null, typeof(EF), nameof(EF.Functions)), member, pattern);
                    filterBody = filterBody == null ? likeCall : Expression.OrElse(filterBody, likeCall);
                }
                catch { continue; }
            }

            if (filterBody == null) return query;
            return query.Where(Expression.Lambda<Func<T, bool>>(filterBody, param));
        }

        private static IQueryable<T> ApplySort<T>(IQueryable<T> query, string? sortBy, string dir)
        {
            var column = !string.IsNullOrWhiteSpace(sortBy) ? sortBy : "Id";
            var direction = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

            query = query.OrderBy($"{column} {direction}");

            // Secondary sort on Id to prevent rows with the same value from swapping pages.
            // Guard: only apply if the DTO actually has an "Id" property.
            var hasId = typeof(T).GetProperty("Id") != null;
            if (column != "Id" && hasId)
                query = ((IOrderedQueryable<T>)query).ThenBy("Id asc");

            return query;
        }
    }
}