using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Entities;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize]
// Adds/removes a book from the current user's favorites
public class FavoritesController : Controller
{
    private readonly BookstoreContext _db;
    public FavoritesController(BookstoreContext db) => _db = db;

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int bookId, string? returnUrl = null)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(idStr, out var userId)) return Forbid();

        var fav = await _db.Favorites.FindAsync(userId, bookId);
        if (fav == null)
        {
            _db.Favorites.Add(new Favorite { UserId = userId, BookId = bookId });
            TempData["Toast"] = "Added to favourites.";
        }
        else
        {
            _db.Favorites.Remove(fav);
            TempData["Toast"] = "Removed from favourites.";
        }
        await _db.SaveChangesAsync();

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Home");
    }
}
