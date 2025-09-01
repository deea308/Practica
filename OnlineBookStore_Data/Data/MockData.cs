using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlineBookStore_Entities;

namespace OnlineBookStore_Data.Data
{
    // Simple in-memory sample data for testing/demo (not EF seeding)
    public static class MockData
    {
        public static List<Publisher> Publisher = new List<Publisher>
        {
            new Publisher {PublisherId=1, Name= "penguin Books"},
            new Publisher {PublisherId=2, Name="HarperCollins" }
        };

        public static List<Author> Authors = new List<Author>
        {
             new Author { AuthorId = 1, Name = "Liz Tomforde" },
            new Author { AuthorId = 2, Name = "J.K. Rowling" },
            new Author { AuthorId = 3, Name = "Suzzana Collins" }
        };

        public static List<User> Users = new List<User>
        {
            new User { UserId = 1, Username = "andreea", Email = "andreea@gmail.com" },
            new User { UserId = 2, Username = "denisa", Email = "denisa@gmail.com" },
            new User { UserId = 2, Username = "alessia", Email = "alessia@gmail.com" }
        };

        public static List<Genre> Genres = new List<Genre>
        {
            new Genre
            {
                GenreId = 1,
                Name = "Science Fiction",
                Books = new List<Book>()
            },
            new Genre
            {
                GenreId = 2,
                Name = "Fantasy",
                Books = new List<Book>()
            },
            new Genre
            {
                GenreId = 3,
                Name = "Romance",
                Books = new List<Book>()
            }
        };

        public static List<Book> Books = new List<Book>
        {
            new Book
            {
                BookId = 1,
                Title = "Mile High",
                Description = "Romance novel by Liz Tomforde",
                Price = 9.99m,
                GenreId = 3,
                PublisherId = 1,
                AuthorId=1,
                Reviews = new List<Review>
                {
                    new Review { ReviewId = 1, Content = "Great book!", Rating = 5, BookId = 1}
                }
            },
            new Book
            {
                BookId = 2,
                Title = "Harry Potter",
                Description = "Fantasy novel by J.K. Rowling",
                Price = 14.99m,
                GenreId = 2,
                PublisherId = 2,
                AuthorId=2,
                Reviews = new List<Review>
                {
                    new Review { ReviewId = 2, Content = "Magical!", Rating = 4, BookId = 2}
                }
            },
            new Book
            {
                BookId = 3,
                Title = "Hunger Games",
                Description = "Classic fantasy by Suzzana Collins",
                Price = 12.50m,
                GenreId = 2,
                PublisherId = 1,
                AuthorId=2,
                Reviews = new List<Review>()
            }
        };

        static MockData()
        {
            Genres.Find(g => g.GenreId == 2)?.Books.Add(Books[1]);
            Genres.Find(g => g.GenreId == 2)?.Books.Add(Books[2]);
            Genres.Find(g => g.GenreId == 3)?.Books.Add(Books[0]);
        }
    }
}
