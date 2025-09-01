using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Entities;

namespace OnlineBookStore_Data.Repository
{
    public class UserRepository : GenericRepository<User>
    {
        public UserRepository(BookstoreContext context) : base(context) { }

        // Get all users with their reviews
        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _dbSet
                .Include(u => u.Reviews)
                .AsNoTracking()
                .ToListAsync();
        }

        // Get one user by id with their reviews
        public override async Task<User?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(u => u.Reviews)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        // Search users by username, email, or review content (paged)
        public async Task<(List<User> Items, int Total)> SearchPagedAsync(
            string? q, int page, int pageSize)
        {
            var query = _dbSet
                .Include(u => u.Reviews)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(u =>
                    EF.Functions.Like(u.Username!, $"%{term}%") ||
                    EF.Functions.Like(u.Email!, $"%{term}%") ||
                    u.Reviews.Any(r => EF.Functions.Like(r.Content!, $"%{term}%")));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(u => u.Username)
                .ThenBy(u => u.UserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
