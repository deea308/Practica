using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Entities;

namespace OnlineBookStore_Data.Repository
{
    public class BookRepository : GenericRepository<Book>
    {
        public BookRepository(BookstoreContext context) : base(context) { }

        // Get all books with Author, Genre, and Publisher included
        public async Task<List<Book>> GetAllWithDetailsAsync()
        {
            return await _dbSet
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .Include(b => b.Publisher)
                .AsNoTracking()
                .ToListAsync();
        }

        // Get a single book by ID with Author, Publisher, Genre, and Reviews
        public async Task<Book?> GetByIdWithDetailsAsync(int id)
        {
            return await _dbSet
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Genre)
                .Include(b => b.Reviews)
                .FirstOrDefaultAsync(b => b.BookId == id);
        }

        // Search books by title or author name with pagination
        public async Task<(List<Book> Items, int Total)> SearchPagedAsync(
            string? q, int page, int pageSize)
        {
            var query = _dbSet
                .Include(b => b.Author)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(b =>
                    EF.Functions.Like(b.Title, $"%{term}%") ||
                    (b.Author != null && EF.Functions.Like(b.Author.Name!, $"%{term}%")));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public override async Task<IEnumerable<Book>> GetAllAsync()
            => await GetAllWithDetailsAsync();

        // Override to include Author, Publisher, Genre, and Reviews with User
        public override async Task<Book?> GetByIdAsync(int id)
        {
            return await _dbSet
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .Include(b => b.Genre)
                .Include(b => b.Reviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(b => b.BookId == id);
        }

        // Extended search: filter by genre and price range
        public async Task<(List<Book> Items, int Total)> SearchPagedAsync(
            string? q, int page, int pageSize,
            int? genreId = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var query = _dbSet
                .Include(b => b.Author)
                .Include(b => b.Genre)
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(b =>
                    EF.Functions.Like(b.Title, $"%{term}%") ||
                    (b.Author != null && EF.Functions.Like(b.Author.Name!, $"%{term}%")));
            }

            if (genreId.HasValue)
                query = query.Where(b => b.GenreId == genreId.Value);

            if (minPrice.HasValue)
                query = query.Where(b => b.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(b => b.Price <= maxPrice.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

    }
}
