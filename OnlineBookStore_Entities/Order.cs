using System;
using System.Collections.Generic;

namespace OnlineBookStore_Entities
{
    /// Represents a customer order including purchaser details, shipping information,
    /// payment method, order items, and order status.
    public class Order
    {
        public int OrderId { get; set; }

        public int UserId { get; set; }
        public User? User { get; set; }   

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string PaymentMethod { get; set; } = "Cash on delivery";

        public string ShipToName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        public decimal Total { get; set; }
        public string Status { get; set; } = "Pending";

        public List<OrderItem> Items { get; set; } = new();
    }
}
