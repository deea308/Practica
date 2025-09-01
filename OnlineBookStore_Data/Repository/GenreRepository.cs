using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Entities;

namespace OnlineBookStore_Data.Repository
{
    public class GenreRepository : GenericRepository<Genre>
    {
        public GenreRepository(BookstoreContext context) : base(context) { }

        // Get all genres with their books
        public override async Task<IEnumerable<Genre>> GetAllAsync()
        {
            return await _dbSet
                .Include(g => g.Books)
                .AsNoTracking()
                .ToListAsync();
        }

        // Get one genre by id with its books
        public override async Task<Genre?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(g => g.Books)
                .FirstOrDefaultAsync(g => g.GenreId == id);
        }

        // Search genres by name or any contained book title (paged)
        public async Task<(List<Genre> Items, int Total)> SearchPagedAsync(
            string? q, int page, int pageSize)
        {
            var query = _dbSet
                .Include(g => g.Books)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(g =>
                    EF.Functions.Like(g.Name!, $"%{term}%") ||
                    g.Books.Any(b => EF.Functions.Like(b.Title!, $"%{term}%")));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(g => g.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
