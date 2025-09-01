using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using System.Security.Claims;

[Authorize]
// Createreviews; admins can manage all, users only their own
public class ReviewsController : Controller
{
    private readonly ReviewRepository _reviews;
    private readonly BookstoreContext _db;

    public ReviewsController(ReviewRepository reviews, BookstoreContext db)
    {
        _reviews = reviews;
        _db = db;
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var items = await _reviews.GetAllAsync(); 
        return View(items);                       
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var review = await _reviews.GetByIdAsync(id);
        if (review == null) return NotFound();
        return View(review); 
    }

    private async Task<int?> GetCurrentUserIdAsync()
    {
        var idClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(idClaim, out var asInt))
            return asInt;

        var username = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username)) return null;

        return await _db.Users
            .Where(u => u.Username == username)
            .Select(u => (int?)u.UserId)
            .FirstOrDefaultAsync();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromForm] int bookId,
                                            [FromForm] int rating,
                                            [FromForm] string content)
    {
        if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(content))
        {
            TempData["Error"] = "Please give a rate between 1-5 and leave a comment";
            return RedirectToAction("BookDetails", "Home", new { id = bookId });
        }

        var userId = await GetCurrentUserIdAsync();
        if (userId is null)
        {
            TempData["Error"] = "Coudn't identify the current user";
            return RedirectToAction("BookDetails", "Home", new { id = bookId });
        }

        var existing = await _db.Reviews.FirstOrDefaultAsync(r => r.BookId == bookId && r.UserId == userId.Value);
        if (existing != null)
        {
            existing.Rating = rating;
            existing.Content = content.Trim();
            await _reviews.UpdateAsync(existing);
            TempData["Success"] = "Your review was updated";
        }
        else
        {
            await _reviews.AddAsync(new Review
            {
                BookId = bookId,
                UserId = userId.Value,
                Rating = rating,
                Content = content.Trim()
            });
            TempData["Success"] = "Thank you for your review";
        }

        return RedirectToAction("BookDetails", "Home", new { id = bookId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update([FromForm] int id,
                                            [FromForm] int rating,
                                            [FromForm] string content,
                                            [FromForm] int? bookId)
    {
        var review = await _reviews.GetByIdAsync(id);
        if (review == null) return NotFound();

        var currentUserId = await GetCurrentUserIdAsync();
        var isAdmin = User.HasClaim("IsAdmin", "true");
        if (!isAdmin && currentUserId != review.UserId) return Forbid();

        if (rating < 1 || rating > 5 || string.IsNullOrWhiteSpace(content))
        {
            TempData["Error"] = "Please give a rate between 1-5 and leave a comment";
            return bookId.HasValue
                ? RedirectToAction("BookDetails", "Home", new { id = bookId.Value })
                : RedirectToAction(nameof(Index));
        }

        review.Rating = rating;
        review.Content = content.Trim();
        await _reviews.UpdateAsync(review);

        TempData["Success"] = "The review is up to date";
        return bookId.HasValue
            ? RedirectToAction("BookDetails", "Home", new { id = bookId.Value })
            : RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromForm] int id, [FromForm] int? bookId)
    {
        var review = await _reviews.GetByIdAsync(id);
        if (review == null) return NotFound();

        var currentUserId = await GetCurrentUserIdAsync();
        var isAdmin = User.HasClaim("IsAdmin", "true");
        if (!isAdmin && currentUserId != review.UserId) return Forbid();

        await _reviews.DeleteAsync(id);

        TempData["Success"] = "The review was deleted";
        return bookId.HasValue
            ? RedirectToAction("BookDetails", "Home", new { id = bookId.Value })
            : RedirectToAction(nameof(Index));
    }
}
