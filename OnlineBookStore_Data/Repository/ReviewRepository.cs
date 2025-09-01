using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBookStore_Data.Repository
{
    public class ReviewRepository : GenericRepository<Review>
    {
        public ReviewRepository(BookstoreContext context) : base(context) { }

        // Get all reviews with their Book and User
        public override async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await _dbSet
                .Include(r => r.Book)
                .Include(r => r.User) 
                .AsNoTracking()
                .ToListAsync();
        }

        // Get one review by id with Book and User
        public override async Task<Review?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(r => r.Book)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.ReviewId == id);
        }

        // Search reviews by book title or username (paged)
        public async Task<(List<Review> Items, int Total)> SearchPagedAsync(
            string? q, int page, int pageSize)
        {
            var query = _dbSet
                .Include(r => r.Book)
                .Include(r => r.User)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(r =>
                    (r.Book != null && EF.Functions.Like(r.Book.Title!, $"%{term}%")) ||
                    (r.User != null && EF.Functions.Like(r.User.Username!, $"%{term}%"))
                );
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderByDescending(r => r.ReviewId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
