using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Data;

namespace OnlineBookStore_Data.Repository
{
    // Default EF Core implementation of the generic repository pattern
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly BookstoreContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(BookstoreContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
            => await _dbSet.ToListAsync();

        public virtual async Task<T?> GetByIdAsync(int id)
            => await _dbSet.FindAsync(id);

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }
    }
}
