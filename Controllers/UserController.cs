using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookstore.Service;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Entities;
using System.Security.Claims;

[Authorize]
// Admin user management + user self-profile (avatar, favorites, orders)
public class UserController : Controller
{
    private readonly UserService _users;
    private readonly BookstoreContext _db;
    private readonly IWebHostEnvironment _env;

    public UserController(BookstoreContext db, IWebHostEnvironment env, UserService users)
    {
        _db = db;
        _env = env;
        _users = users;
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

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Index()
    {
        var items = await _users.GetAllUsersAsync();
        return View(items);
    }

    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Details(int id)
    {
        var user = await _users.GetUserAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet]
    public IActionResult Create() => View(new User());

    [Authorize(Policy = "AdminOnly")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(User user)
    {
        if (!ModelState.IsValid) return View(user);
        await _users.AddUserAsync(user);
        TempData["Toast"] = "User created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _users.GetUserAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("UserId,Username,Email,IsAdmin")] User input)
    {
        if (id != input.UserId) return BadRequest();

        var user = await _users.GetUserAsync(id);
        if (user == null) return NotFound();

        user.Username = (input.Username ?? "").Trim();
        user.Email = (input.Email ?? "").Trim().ToLowerInvariant();
        user.IsAdmin = input.IsAdmin;

        await _users.UpdateUserAsync(user);
        TempData["Toast"] = "User updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Policy = "AdminOnly")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var u = await _users.GetUserAsync(id);
        if (u == null) return NotFound();

        DeletePhysicalFileIfExists(u.AvatarPath);

        var carts = await _db.Carts.Where(c => c.UserId == id).ToListAsync();
        _db.Carts.RemoveRange(carts);
        await _db.SaveChangesAsync();

        await _users.DeleteUserAsync(id);

        TempData["Toast"] = "User and all related data deleted.";
        return RedirectToAction(nameof(Index));
    }

    private void DeletePhysicalFileIfExists(string? webPath)
    {
        if (string.IsNullOrWhiteSpace(webPath)) return;
        var full = Path.Combine(_env.WebRootPath, webPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (System.IO.File.Exists(full)) System.IO.File.Delete(full);
    }

    [HttpGet("User/Profile")]
    public async Task<IActionResult> Profile()
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return Forbid();

        var me = await _users.GetUserAsync(userId.Value);
        if (me == null) return NotFound();

        var myOrders = await _db.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == me.UserId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        var favBooks = await _db.Favorites
            .Where(f => f.UserId == me.UserId)
            .Include(f => f.Book)
            .OrderByDescending(f => f.CreatedUtc)
            .Select(f => f.Book!)
            .ToListAsync();

        ViewBag.Orders = myOrders;
        ViewBag.FavoriteBooks = favBooks;
        return View(me);
    }

    [HttpPost("User/UploadAvatar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadAvatar(IFormFile? avatar)
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return Forbid();

        var me = await _users.GetUserAsync(userId.Value);
        if (me == null) return NotFound();

        if (avatar is null || avatar.Length == 0)
        {
            TempData["ToastError"] = "Please choose an image.";
            return RedirectToAction(nameof(Profile));
        }

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(avatar.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext) || avatar.Length > 2 * 1024 * 1024)
        {
            TempData["ToastError"] = "Only .jpg/.jpeg/.png/.webp up to 2 MB.";
            return RedirectToAction(nameof(Profile));
        }

        var folder = Path.Combine(_env.WebRootPath, "uploads", "avatars");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(folder, fileName);
        using (var stream = System.IO.File.Create(fullPath))
            await avatar.CopyToAsync(stream);

        DeletePhysicalFileIfExists(me.AvatarPath);
        me.AvatarPath = $"/uploads/avatars/{fileName}";

        await _users.UpdateUserAsync(me);
        TempData["Toast"] = "Avatar updated.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpPost("User/RemoveAvatar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAvatar()
    {
        var userId = await GetCurrentUserIdAsync();
        if (userId is null) return Forbid();

        var me = await _users.GetUserAsync(userId.Value);
        if (me == null) return NotFound();

        DeletePhysicalFileIfExists(me.AvatarPath);
        me.AvatarPath = null;

        await _users.UpdateUserAsync(me);
        TempData["Toast"] = "Avatar removed.";
        return RedirectToAction(nameof(Profile));
    }
}
