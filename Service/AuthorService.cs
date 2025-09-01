using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineBookstore.Service
{
    // Provides high-level operations for Authors
    public class AuthorService
    {
        private readonly AuthorRepository _authors;

        public AuthorService(AuthorRepository authors)
        {
            _authors = authors;
        }

        public Task<IEnumerable<Author>> GetAllAuthorsAsync()
            => _authors.GetAllAsync(); 

        public Task<Author?> GetAuthorAsync(int id)
            => _authors.GetByIdAsync(id); 

        public Task AddAuthorAsync(Author author)
            => _authors.AddAsync(author);

        public Task UpdateAuthorAsync(Author author)
            => _authors.UpdateAsync(author);

        public Task DeleteAuthorAsync(int id)
            => _authors.DeleteAsync(id);
    }
}
