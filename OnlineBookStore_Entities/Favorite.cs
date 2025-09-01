using System;

namespace OnlineBookStore_Entities
{
    /// Represents a book that a user has marked as a favorite for quick access.
    public class Favorite
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public Book? Book { get; set; }
    }
}
