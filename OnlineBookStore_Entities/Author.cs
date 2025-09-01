using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBookStore_Entities
{
    /// Represents a content creator (writer) responsible for one or more books.
    public class Author
    {
        public int AuthorId { get; set; }
        public string? Name { get; set; }
        public List<Book> Books { get; set; } = new List<Book>();
    }
}
