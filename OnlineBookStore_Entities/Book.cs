using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBookStore_Entities
{

    /// Represents a book sold in the online store, including its pricing, metadata,
    /// and relationships to author, publisher, genre, and reviews.
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }

        public int GenreId { get; set; }
        public Genre? Genre { get; set; }

        public int PublisherId { get; set; }
        public Publisher? Publisher { get; set; }

        public int AuthorId { get; set; }
        public Author? Author { get; set; }

        public List<Review> Reviews { get; set; } = new List<Review>();

        public string? CoverImagePath { get; set; }
    }
}
