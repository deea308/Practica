using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBookStore_Entities
{

    /// Represents a user-submitted review for a book, including rating and text.
    public class Review
    {
        public int ReviewId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public int BookId { get; set; }
        public Book? Book { get; set; }

   
        public int UserId { get; set; }
        public User? User { get; set; }     



    }
}
