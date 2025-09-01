using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Data;
using OnlineBookstore.ViewModels;

[Authorize(Policy = "AdminOnly")]
// Admin dashboard showing entity counts
public class DashboardController : Controller
{
    private readonly BookstoreContext _db;
    public DashboardController(BookstoreContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var vm = new DashboardStatsVm
        {
            BooksCount = await _db.Books.CountAsync(),
            AuthorsCount = await _db.Authors.CountAsync(),
            GenresCount = await _db.Genres.CountAsync(),
            PublishersCount = await _db.Publishers.CountAsync(),
            ReviewsCount = await _db.Reviews.CountAsync(),
            UsersCount = await _db.Users.CountAsync()
        };

        return View(vm); 
    }
}
