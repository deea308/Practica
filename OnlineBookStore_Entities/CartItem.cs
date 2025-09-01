using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBookStore_Entities
{
    /// Represents a single book entry in a shopping cart, including the quantity
    /// and the price at the time the item was added.
    public class CartItem
    {
        public int CartItemId { get; set; }
        public Guid CartId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal PriceAtAdd { get; set; }  
        public Cart? Cart { get; set; }
    }
}
