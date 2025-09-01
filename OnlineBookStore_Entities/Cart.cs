using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineBookStore_Entities
{
    /// Represents a shopping cart that may belong to a registered user or a guest,
    /// containing multiple cart items.
    public class Cart
    {
        public Guid CartId { get; set; }        
        public int? UserId { get; set; }        
  
        public List<CartItem> Items { get; set; } = new();
    }
}
