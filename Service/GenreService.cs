using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;

namespace OnlineBookstore.Service
{
    // Provides high-level operations for Genres
    public class GenreService
    {
        private readonly GenreRepository _genres;

        public GenreService(GenreRepository genres)
        {
            _genres = genres;
        }

        public Task<IEnumerable<Genre>> GetAllGenresAsync()
            => _genres.GetAllAsync(); 

        public Task<Genre?> GetGenreAsync(int id)
            => _genres.GetByIdAsync(id); 

        public Task AddGenreAsync(Genre genre)
            => _genres.AddAsync(genre);

        public Task UpdateGenreAsync(Genre genre)
            => _genres.UpdateAsync(genre);

        public Task DeleteGenreAsync(int id)
            => _genres.DeleteAsync(id);
    }
}
