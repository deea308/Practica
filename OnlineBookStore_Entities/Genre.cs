using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBookStore_Entities
{

    /// Represents a classification for books (e.g., Fantasy, History).
    public class Genre
    {
        public int GenreId { get; set; }
        public string Name { get; set; }
        public List<Book> Books { get; set; } = new List<Book>();
    }
}
