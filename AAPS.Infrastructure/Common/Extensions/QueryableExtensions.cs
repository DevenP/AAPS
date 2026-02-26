using AAPS.Application.Common.Paging;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;

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
            // 1. Dynamic Search (Global)
            if (performSearch && !string.IsNullOrWhiteSpace(request.Search))
            {
                query = ApplySearch(query, request.Search.Trim());
            }

            // 2. NEW: Advanced Column Filtering (Includes Date Ranges)
            if (request.ColumnFilters != null && request.ColumnFilters.Any())
            {
                query = ApplyColumnFilters(query, request.ColumnFilters);
            }

            // 3. Dynamic Sorting
            if (!string.IsNullOrWhiteSpace(request.SortBy))
                query = ApplySort(query, request.SortBy, request.SortDir);

            // 4. Totals and Paging
            var totalCount = await query.CountAsync(ct);

            if (request.PageSize == -1)
            {
                var allItems = await query.ToListAsync(ct);
                return new Application.Common.Paging.PagedResult<T>(allItems, 1, totalCount, totalCount);
            }

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
                if (string.IsNullOrWhiteSpace(filter.Value)) continue;

                // 1. Clean the key to find the real property name
                var cleanKey = filter.Key.Replace("_From", "").Replace("_To", "");
                var prop = properties.FirstOrDefault(p => p.Name.Equals(cleanKey, StringComparison.OrdinalIgnoreCase));

                if (prop == null) continue;

                var type = prop.PropertyType;

                // 2. Handle BOOLEANS (Generic Detection)
                if (type == typeof(bool) || type == typeof(bool?))
                {
                    if (bool.TryParse(filter.Value, out var boolValue))
                    {
                        query = query.Where($"{prop.Name} == @0", boolValue);
                    }
                }
                // 3. Handle DATE RANGES (From/To)
                else if (type == typeof(DateTime) || type == typeof(DateTime?))
                {
                    if (filter.Key.EndsWith("_From") && DateTime.TryParse(filter.Value, out var fromDate))
                    {
                        query = query.Where($"{prop.Name} != null && {prop.Name} >= @0", fromDate);
                    }
                    else if (filter.Key.EndsWith("_To") && DateTime.TryParse(filter.Value, out var toDate))
                    {
                        var endOfDay = toDate.Date.AddDays(1).AddTicks(-1);
                        query = query.Where($"{prop.Name} != null && {prop.Name} <= @0", endOfDay);
                    }
                }
                // 4. Handle STRINGS (Including null/notnull keywords)
                else if (type == typeof(string))
                {
                    var val = filter.Value.ToLower();
                    if (val == "null")
                    {
                        // Re-added your specific IS NULL OR EMPTY logic
                        query = query.Where($"string.IsNullOrEmpty({prop.Name}) || {prop.Name}.Trim() == \"\"");
                    }
                    else if (val == "notnull")
                    {
                        query = query.Where($"!string.IsNullOrEmpty({prop.Name}) && {prop.Name}.Trim() != \"\"");
                    }
                    else
                    {
                        query = query.Where($"{prop.Name}.Contains(@0)", filter.Value);
                    }
                }
            }
            return query;
        }


        //private static IQueryable<T> ApplyColumnFilters<T>(IQueryable<T> query, Dictionary<string, string> filters)
        //{
        //    foreach (var filter in filters)
        //    {
        //        if (string.IsNullOrWhiteSpace(filter.Value)) continue;

        //        Handle Date Range: FROM
        //        if (filter.Key.EndsWith("_From") && DateTime.TryParse(filter.Value, out var fromDate))
        //        {
        //            var propertyName = filter.Key.Substring(0, filter.Key.Length - 5);
        //            Added null check: Property != null AND Property >= fromDate
        //            query = query.Where($"{propertyName} != null && {propertyName} >= @0", fromDate);
        //        }
        //        Handle Date Range: TO
        //        else if (filter.Key.EndsWith("_To") && DateTime.TryParse(filter.Value, out var toDate))
        //        {
        //            var propertyName = filter.Key.Substring(0, filter.Key.Length - 3);
        //            var endOfDay = toDate.Date.AddDays(1).AddTicks(-1);
        //            Added null check: Property != null AND Property <= endOfDay
        //            query = query.Where($"{propertyName} != null && {propertyName} <= @0", endOfDay);
        //        }
        //        else
        //        {
        //        Important: Don't process the _From/_To keys again here
        //            if (filter.Key.EndsWith("_From") || filter.Key.EndsWith("_To")) continue;

        //            if (filter.Value.ToLower() == "null")
        //            {
        //                This catches: IS NULL OR = "" OR = " "
        //                query = query.Where($"string.IsNullOrEmpty({filter.Key}) || {filter.Key}.Trim() == \"\"");
        //            }
        //            else if (filter.Value.ToLower() == "notnull")
        //            {
        //                This catches anything that HAS actual text
        //               query = query.Where($"!string.IsNullOrEmpty({filter.Key}) && {filter.Key}.Trim() != \"\"");
        //            }
        //            else
        //            {
        //                Standard search for specific text

        //               query = query.Where($"{filter.Key}.Contains(@0)", filter.Value);
        //            }
        //        }
        //    }
        //    return query;
        //}

        //private static IQueryable<T> ApplyColumnFilters<T>(IQueryable<T> query, Dictionary<string, string> filters)
        //{
        //    foreach (var filter in filters)
        //    {
        //        if (string.IsNullOrWhiteSpace(filter.Value)) continue;

        //        // Handle Date Range: FROM
        //        if (filter.Key.EndsWith("_From") && DateTime.TryParse(filter.Value, out var fromDate))
        //        {
        //            // Extract the real property name (e.g., "DateOfService")
        //            var propertyName = filter.Key.Substring(0, filter.Key.Length - 5);
        //            query = query.Where($"{propertyName} >= @0", fromDate);
        //        }
        //        // Handle Date Range: TO
        //        else if (filter.Key.EndsWith("_To") && DateTime.TryParse(filter.Value, out var toDate))
        //        {
        //            // Extract the real property name (e.g., "DateOfService")
        //            var propertyName = filter.Key.Substring(0, filter.Key.Length - 3);
        //            var endOfDay = toDate.Date.AddDays(1).AddTicks(-1);
        //            query = query.Where($"{propertyName} < @0", endOfDay);
        //        }
        //        // Handle standard String/Null filters
        //        else
        //        {
        //            // Important: Don't process the _From/_To keys again here
        //            if (filter.Key.EndsWith("_From") || filter.Key.EndsWith("_To")) continue;

        //            if (filter.Value == "null")
        //                query = query.Where($"{filter.Key} == null");
        //            else if (filter.Value == "notnull")
        //                query = query.Where($"{filter.Key} != null");
        //            else
        //                // Ensure the property exists on the T type before calling Contains
        //                query = query.Where($"{filter.Key}.Contains(@0)", filter.Value);
        //        }
        //    }
        //    return query;
        //}

        private static IQueryable<T> ApplySearch<T>(IQueryable<T> query, string term)
        {
            var param = Expression.Parameter(typeof(T), "e");
            var stringProperties = typeof(T).GetProperties()
             .Where(p => p.PropertyType == typeof(string) &&
                        (p.GetCustomAttribute<BrowsableAttribute>()?.Browsable ?? false == false));

            Expression? filterBody = null;
            var pattern = Expression.Constant($"%{term}%");
            var likeMethod = typeof(DbFunctionsExtensions).GetMethod("Like", new[] { typeof(DbFunctions), typeof(string), typeof(string) });

            foreach (var prop in stringProperties)
            {
                //var member = Expression.Property(param, prop);
                //var likeCall = Expression.Call(null, likeMethod!, Expression.Property(null, typeof(EF), nameof(EF.Functions)), member, pattern);
                //filterBody = filterBody == null ? likeCall : Expression.OrElse(filterBody, likeCall);
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
            //var column = !string.IsNullOrWhiteSpace(sortBy) ? sortBy : typeof(T).GetProperties().FirstOrDefault()?.Name;
            //if (string.IsNullOrEmpty(column)) return query;
            //var direction = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
            //return query.OrderBy($"{column} {direction}");

            // 1. If user clicked a column, use that.
            // 2. If not, use "Id" (or your primary key name) as the absolute fallback.
            var column = !string.IsNullOrWhiteSpace(sortBy)
                         ? sortBy
                         : "Id"; // Change "Id" to "Provider_Id" or "Sesis_Id" if your DTOs use different names

            var direction = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

            // Apply the primary sort
            query = query.OrderBy($"{column} {direction}");

            // 3. IMPORTANT: Add a secondary sort on ID to break ties
            // This prevents rows with the same name/date from swapping places
            if (column != "Id")
            {
                query = ((IOrderedQueryable<T>)query).ThenBy("Id asc");
            }

            return query;
        }
    }
}
