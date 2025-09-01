namespace OnlineBookStore_Entities
{
    /// Represents a line item within an order, linking a purchased book to its order
    /// with quantity and unit price information.
    public class OrderItem
    {
        public int OrderItemId { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }

        public int BookId { get; set; }              
        public string BookTitle { get; set; } = "";  
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
