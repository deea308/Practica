using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBookStore_Data.Repository
{
    public class PublisherRepository : GenericRepository<Publisher>
    {
        public PublisherRepository(BookstoreContext context) : base(context) { }

        // Get all publishers
        public override async Task<IEnumerable<Publisher>> GetAllAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .ToListAsync();
        }

        // Get one publisher by id
        public override async Task<Publisher?> GetByIdAsync(int id)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.PublisherId == id);
        }

        // Search publishers by name (paged)
        public async Task<(List<Publisher> Items, int Total)> SearchPagedAsync(
            string? q, int page, int pageSize)
        {
            var query = _dbSet.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(p => EF.Functions.Like(p.Name!, $"%{term}%"));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}