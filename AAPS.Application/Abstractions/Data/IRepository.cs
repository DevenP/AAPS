namespace AAPS.Application.Abstractions.Data
{
    public interface IRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);

        Task<IEnumerable<T>> GetAllAsync();

        // Paging support
        Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);

        Task CreateAsync(T entity);

        Task Update(T entity);

        Task Delete(T entity);

        /// <summary>
        /// Bypasses trackers
        /// </summary>
        /// <param name="entities"></param>
        /// <returns></returns>
        Task BulkInsertAsync(IList<T> entities);

    }
}
