using AAPS.Application.Common.Paging;
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
            CancellationToken ct = default) where T : class
        {
            // 1. Dynamic Search
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = ApplySearch(query, request.Search.Trim());
            }

            // 2. Dynamic Sorting
            if (!string.IsNullOrWhiteSpace(request.SortBy))
                query = ApplySort(query, request.SortBy, request.SortDir);

            // 3. Totals and Paging
            var totalCount = await query.CountAsync(ct);

            // If PageSize is -1, skip the Skip/Take and return everything
            if (request.PageSize == -1)
            {
                var allItems = await query.ToListAsync(ct);
                return new Application.Common.Paging.PagedResult<T>(allItems, 1, totalCount, totalCount);
            }

            // 4. Page LAST (only take the small slice needed for the UI)
            var page = request.Page < 1 ? 1 : request.Page;

            var pageSize = request.PageSize <= 0 ? 25 : request.PageSize;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new Application.Common.Paging.PagedResult<T>(items, page, pageSize, totalCount);
        }

        private static IQueryable<T> ApplySearch<T>(IQueryable<T> query, string term)
        {
            var param = Expression.Parameter(typeof(T), "e");

            // Find all string properties on the entity
            var stringProperties = typeof(T).GetProperties()
                .Where(p => p.PropertyType == typeof(string));

            Expression? filterBody = null;
            var pattern = Expression.Constant($"%{term}%");

            // Access EF.Functions.Like via Reflection for the SQL 'LIKE' translation
            var likeMethod = typeof(DbFunctionsExtensions).GetMethod("Like",
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

            foreach (var prop in stringProperties)
            {
                var member = Expression.Property(param, prop);
                var likeCall = Expression.Call(null, likeMethod!,
                    Expression.Property(null, typeof(EF), nameof(EF.Functions)), member, pattern);

                // Combine with OR logic: (Col1 LIKE %term% OR Col2 LIKE %term% ...)
                filterBody = filterBody == null ? likeCall : Expression.OrElse(filterBody, likeCall);
            }

            if (filterBody == null) return query;

            return query.Where(Expression.Lambda<Func<T, bool>>(filterBody, param));
        }


        private static IQueryable<T> ApplySort<T>(IQueryable<T> query, string? sortBy, string dir)
        {
            //if (string.IsNullOrWhiteSpace(sortBy)) return query;

            //// Using System.Linq.Dynamic.Core allows string-based sorting: "Name descending"
            //var sortExpression = $"{sortBy} {(dir.ToLower() == "desc" ? "descending" : "ascending")}";
            //return query.OrderBy(sortExpression);

            // 1. Determine Column: Use provided string OR reflect the first property of the class
            var column = !string.IsNullOrWhiteSpace(sortBy)
                         ? sortBy
                         : typeof(T).GetProperties().FirstOrDefault()?.Name;

            if (string.IsNullOrEmpty(column)) return query;

            // 2. Determine Direction: Map to "desc" or "asc" (Dynamic LINQ handles both)
            var direction = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";

            // 3. Execute: This creates the "ORDER BY Column Dir" SQL
            return query.OrderBy($"{column} {direction}");
        }

        //private static IQueryable<T> ApplySearch<T>(IQueryable<T> query, string term)
        //{
        //    var param = Expression.Parameter(typeof(T), "e");

        //    var properties = typeof(T).GetProperties()
        //        .Where(p => p.PropertyType == typeof(string));

        //    Expression? body = null;
        //    var pattern = Expression.Constant($"%{term}%");
        //    var likeMethod = typeof(DbFunctionsExtensions).GetMethod("Like",
        //        new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        //    foreach (var prop in properties)
        //    {
        //        var member = Expression.Property(param, prop);
        //        var likeCall = Expression.Call(null, likeMethod!,
        //            Expression.Property(null, typeof(EF), nameof(EF.Functions)), member, pattern);
        //        body = body == null ? likeCall : Expression.OrElse(body, likeCall);
        //    }

        //    return body == null ? query : query.Where(Expression.Lambda<Func<T, bool>>(body, param));
        //}
    }

}

