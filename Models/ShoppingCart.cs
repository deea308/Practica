using Newtonsoft.Json;

namespace OnlineBookstore.Models
{
    // Represents a single cart item stored in session (not the database)
    public static class ShoppingCart
    {
        private const string CartSessionKey = "Cart";

        // Retrieve the cart from session (returns empty list if none found)
        public static List<SessionCartItem> GetCart(ISession session)
        {
            var data = session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(data)
                ? new List<SessionCartItem>()
                : JsonConvert.DeserializeObject<List<SessionCartItem>>(data)!;
        }

        // Save the cart to session
        public static void SaveCart(ISession session, List<SessionCartItem> cart)
        {
            var jsonData = JsonConvert.SerializeObject(cart);
            session.SetString(CartSessionKey, jsonData);
        }

        // Add a new item to the cart or update quantity if it already exists
        public static void AddToCart(ISession session, SessionCartItem item)
        {
            var cart = GetCart(session);
            var existing = cart.FirstOrDefault(c => c.BookId == item.BookId);
            if (existing != null)
                existing.Quantity += item.Quantity;
            else
                cart.Add(item);
            SaveCart(session, cart);
        }
    }
}
