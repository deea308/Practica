using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineBookstore.Models;
using OnlineBookstore.Service;
using OnlineBookstore.Utilities;
using OnlineBookstore.Services;
using OnlineBookstore.ViewModels;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Entities;
using System.Security.Claims;

[AllowAnonymous]
// Public/home pages: catalog listing, book details, shopping cart, checkout
public class HomeController : Controller
{
    private readonly BookService _bookService;
    private readonly GenreService _genreService;
    private readonly IEmailSender _email;
    private readonly BookstoreContext _db;

    public HomeController(
       BookService bookService,
       GenreService genreService,
       IEmailSender email,
       BookstoreContext db)
    {
        _bookService = bookService;
        _genreService = genreService;
        _email = email;
        _db = db;
    }

    [AllowAnonymous]
    // Catalog listing with search/filters/pagination
    public async Task<IActionResult> Index(
        string? q, int page = 1,
        int? genreId = null, decimal? minPrice = null, decimal? maxPrice = null)
    {
        if (page < 1) page = 1;
        if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
            (minPrice, maxPrice) = (maxPrice, minPrice);

        const int pageSize = 10;

        var (items, total) = await _bookService
            .SearchBooksPagedAsync(q, page, pageSize, genreId, minPrice, maxPrice);

        var genres = (await _genreService.GetAllGenresAsync())
                     .OrderBy(g => g.Name)
                     .ToList();

        ViewBag.Query = q;
        ViewBag.Page = page;
        ViewBag.PageSize = pageSize;
        ViewBag.Total = total;
        ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);
        ViewBag.SelectedGenreId = genreId;
        ViewBag.GenreSelect = new SelectList(genres, "GenreId", "Name", genreId);
        ViewBag.MinPrice = minPrice;
        ViewBag.MaxPrice = maxPrice;

        var favIds = new List<int>();
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idStr, out var uid))
            favIds = await _db.Favorites
                              .Where(f => f.UserId == uid)
                              .Select(f => f.BookId)
                              .ToListAsync();
        ViewBag.FavIds = favIds;

        return View(items);
    }

    [AllowAnonymous]
    // Show single book page
    public async Task<IActionResult> BookDetails(int id)
    {
        var book = await _bookService.GetBookAsync(id);
        if (book == null) return NotFound();
        return View(book);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Add a book to session cart
    public async Task<IActionResult> AddToCart(int bookId, int quantity = 1)
    {
        var book = await _bookService.GetBookAsync(bookId);
        if (book == null) return NotFound();

        ShoppingCart.AddToCart(HttpContext.Session, new SessionCartItem
        {
            BookId = book.BookId,
            BookTitle = book.Title,
            Quantity = quantity < 1 ? 1 : quantity,
            Price = book.Price
        });

        TempData["Toast"] = "The book was successfully added in the cart";
        return RedirectToAction(nameof(Cart));
    }

    // Show current session cart
    public IActionResult Cart()
    {
        var cart = ShoppingCart.GetCart(HttpContext.Session);
        return View(cart);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Update quantity or remove item from cart
    public IActionResult UpdateCartItem(int bookId, int quantity)
    {
        var cart = ShoppingCart.GetCart(HttpContext.Session);
        var item = cart.FirstOrDefault(c => c.BookId == bookId);
        if (item != null)
        {
            if (quantity <= 0) cart.Remove(item);
            else item.Quantity = quantity;
            ShoppingCart.SaveCart(HttpContext.Session, cart);
        }
        return RedirectToAction(nameof(Cart));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // Remove item from cart
    public IActionResult RemoveFromCart(int bookId)
    {
        var cart = ShoppingCart.GetCart(HttpContext.Session);
        var item = cart.FirstOrDefault(c => c.BookId == bookId);
        if (item != null)
        {
            cart.Remove(item);
            ShoppingCart.SaveCart(HttpContext.Session, cart);
        }
        return RedirectToAction(nameof(Cart));
    }

    [Authorize]
    [HttpGet]
    // Show checkout form
    public IActionResult Checkout()
    {
        var cart = ShoppingCart.GetCart(HttpContext.Session);
        if (cart.Count == 0)
        {
            TempData["ToastError"] = "Your cart is empty.";
            return RedirectToAction(nameof(Cart));
        }

        return View(new CheckoutVm());
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    // Place order from session cart and send confirmation email
    public async Task<IActionResult> Checkout(CheckoutVm vm)
    {
        var cart = ShoppingCart.GetCart(HttpContext.Session);
        if (cart.Count == 0)
            ModelState.AddModelError(string.Empty, "Your cart is empty.");

        if (!ModelState.IsValid) return View(vm);

        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idStr, out var userId))
        {
            TempData["ToastError"] = "We couldn't resolve your account.";
            return RedirectToAction(nameof(Cart));
        }

        var bookIds = cart.Select(c => c.BookId).ToList();
        var prices = await _db.Books
            .Where(b => bookIds.Contains(b.BookId))
            .ToDictionaryAsync(b => b.BookId, b => new { b.Price, b.Title });

        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            PaymentMethod = "Cash on delivery",
            ShipToName = vm.FullName.Trim(),
            Address = vm.Address.Trim(),
            City = vm.City.Trim(),
            PostalCode = vm.PostalCode.Trim(),
            Phone = vm.Phone.Trim(),
            Status = "Pending"
        };

        foreach (var c in cart)
        {
            var p = prices[c.BookId];
            order.Items.Add(new OrderItem
            {
                BookId = c.BookId,
                BookTitle = p.Title,
                Quantity = c.Quantity,
                UnitPrice = p.Price
            });
            order.Total += p.Price * c.Quantity;
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        var email = await GetCurrentUserEmailAsync();
        if (!string.IsNullOrWhiteSpace(email))
        {
            var itemsHtml = string.Join("", order.Items.Select(i =>
                $"<li>{System.Net.WebUtility.HtmlEncode(i.BookTitle)} × {i.Quantity} – {(i.UnitPrice * i.Quantity):C}</li>"));

            var html = $@"
<p>Hi, {System.Net.WebUtility.HtmlEncode(vm.FullName)}!</p>
<p>Thanks for your order — we’re preparing it for dispatch.</p>
<p><strong>Order #</strong>{order.OrderId}<br/>
<strong>Payment:</strong> {order.PaymentMethod}</p>
<p><strong>Shipping to:</strong><br/>
{System.Net.WebUtility.HtmlEncode(order.Address)}<br/>
{System.Net.WebUtility.HtmlEncode(order.City)}, {System.Net.WebUtility.HtmlEncode(order.PostalCode)}<br/>
Phone: {System.Net.WebUtility.HtmlEncode(order.Phone)}</p>
<p><strong>Items</strong></p>
<ul>{itemsHtml}</ul>
<p><strong>Total:</strong> {order.Total:C}</p>
<p>We’ll email you again when it ships.<br/>— OnlineBookstore</p>";

            await _email.SendAsync(email, "Your order is on its way", html);
        }

        ShoppingCart.SaveCart(HttpContext.Session, new List<SessionCartItem>());

        TempData["Toast"] = $"Thank you! Order #{order.OrderId} has been placed.";
        return RedirectToAction(nameof(Index));
    }

    // Try to get current user's email from claims or DB
    private async Task<string?> GetCurrentUserEmailAsync()
    {
        var claimEmail = User.FindFirstValue(ClaimTypes.Email);
        if (!string.IsNullOrWhiteSpace(claimEmail))
            return claimEmail;

        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idStr, out var id))
            return await _db.Users
                .Where(u => u.UserId == id)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

        var username = User.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(username))
            return await _db.Users
                .Where(u => u.Username == username)
                .Select(u => u.Email)
                .FirstOrDefaultAsync();

        return null;
    }

    public IActionResult Privacy() => View();
}
