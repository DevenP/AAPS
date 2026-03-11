using AAPS.Application.Common.Paging;
using AAPS.Application.Common.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;

namespace AAPS.Infrastructure.Common.Extensions
{
    public static class QueryableExtensions
    {
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