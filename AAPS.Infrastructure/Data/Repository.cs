using AAPS.Application.Abstractions.Data;
using AAPS.Infrastructure.Data.Scaffolded;
using EFCore.BulkExtensions;
using Microsoft.EntityFrameworkCore;

namespace AAPS.Infrastructure.Data
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _db;

        private readonly DbSet<T> _dbSet;

        public Repository(AppDbContext db)
        {
            _db = db;
            _dbSet = _db.Set<T>();
        }

        public async Task<T?> GetByIdAsync(int id) 
        {
            return await _dbSet.FindAsync(id);
        } 

        public async Task<IEnumerable<T>> GetAllAsync() 
        {
           return await _dbSet.ToListAsync();
        }

        public async Task<(IEnumerable<T> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
        {
            var count = await _dbSet.CountAsync();

            var items = await _dbSet
                .AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, count);
        }

        public async Task CreateAsync(T entity) 
        {
            await _dbSet.AddAsync(entity);
        } 

        public async Task Update(T entity) 
        { 
            _dbSet.Update(entity);

            await SaveChangesAsync();
        }

        public async Task Delete(T entity) 
        {
            _dbSet.Remove(entity);

            await SaveChangesAsync();
        } 

        public async Task BulkInsertAsync(IList<T> entities)
        {
            // Bypasses the change tracker for "warp speed" performance
            await _db.BulkInsertAsync(entities);
        }

        private async Task SaveChangesAsync()
        {
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Option A: "Database Wins" (Discard your changes and reload from DB)
                // foreach (var entry in ex.Entries) { await entry.ReloadAsync(); }

                // Option B: Throw it back to the UI to let the user decide (Recommended)
                throw;
            }
        }
    }

}
