using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineBookstore.Services
{
    // Provides high-level operations for Books
    public class BookService
    {
        private readonly BookRepository _bookRepository;

        public BookService(BookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        
        public Task<IEnumerable<Book>> GetAllBooksAsync()
            => _bookRepository.GetAllAsync();

        
        public Task<Book?> GetBookAsync(int id)
            => _bookRepository.GetByIdAsync(id);

        public Task AddBookAsync(Book book)
            => _bookRepository.AddAsync(book);

        public Task UpdateBookAsync(Book book)
            => _bookRepository.UpdateAsync(book);

        public Task DeleteBookAsync(int id)
            => _bookRepository.DeleteAsync(id);

        // Search books with pagination + optional filters (genre, price range)
        public Task<(List<Book> Items, int Total)> SearchBooksPagedAsync(
            string? q, int page, int pageSize, int? genreId, decimal? minPrice, decimal? maxPrice)
            => _bookRepository.SearchPagedAsync(q, page, pageSize, genreId, minPrice, maxPrice);

    }
}
