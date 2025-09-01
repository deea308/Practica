using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Entities;

namespace OnlineBookStore_Data.Repository
{
    public class AuthorRepository : GenericRepository<Author>
    {
        public AuthorRepository(BookstoreContext context) : base(context) { }

        // Get all authors with their books
        public override async Task<IEnumerable<Author>> GetAllAsync()
        {
            return await _dbSet
                .Include(a => a.Books)
                .AsNoTracking()
                .ToListAsync();
        }

        // Get one author by id with their books
        public override async Task<Author?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.AuthorId == id);
        }

        // Search authors by name or any of their book titles (paged)
        public async Task<(List<Author> Items, int Total)> SearchPagedAsync(
            string? q, int page, int pageSize)
        {
            var query = _dbSet
                .Include(a => a.Books)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(a =>
                    EF.Functions.Like(a.Name!, $"%{term}%") ||
                    a.Books.Any(b => EF.Functions.Like(b.Title!, $"%{term}%")));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(a => a.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
