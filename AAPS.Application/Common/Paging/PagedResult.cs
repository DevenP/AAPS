namespace AAPS.Application.Common.Paging
{
    public sealed record PagedResult<T>(
        IReadOnlyList<T> Items,
        int Page,
        int PageSize,
        int TotalCount)
    {
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

        public bool HasPreviousPage => Page > 1;

        public bool HasNextPage => Page < TotalPages;

        public PagedResult<TNew> Map<TNew>(Func<T, TNew> transformation)
        {
            var newItems = Items.Select(transformation).ToList();
            return new PagedResult<TNew>(newItems, Page, PageSize, TotalCount);
        }
    }
}
