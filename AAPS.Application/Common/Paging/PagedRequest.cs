namespace AAPS.Application.Common.Paging
{
    public record PagedRequest(
        string? Search = null,
        Dictionary<string, string>? ColumnFilters = null, 
        string? SortBy = null,
        string SortDir = "asc",
        int Page = 1,
        int PageSize = 25);
}
