namespace OnlineBookstore.Models
{
    // Represents a single cart item stored in session
    public class SessionCartItem
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
