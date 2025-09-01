using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBookstore.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    // Admin listing and management of orders
    public class OrdersController : Controller
    {
        private readonly BookstoreContext _db;
        public OrdersController(BookstoreContext db) => _db = db;

        [HttpGet]
        // Paginated order list with optional search and status filter
        public async Task<IActionResult> Index(string? q, string? status = null, int page = 1, int pageSize = 20)
        {
            if (page < 1) page = 1;

            var query = _db.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim();
                query = query.Where(o =>
                    o.OrderId.ToString() == term ||
                    EF.Functions.Like(o.ShipToName!, $"%{term}%") ||
                    EF.Functions.Like(o.Address!, $"%{term}%") ||
                    EF.Functions.Like(o.City!, $"%{term}%") ||
                    EF.Functions.Like(o.PostalCode!, $"%{term}%") ||
                    (o.User != null && EF.Functions.Like(o.User.Email!, $"%{term}%"))
                );
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            ViewBag.Query = q;
            ViewBag.Status = status;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.TotalPages = (int)Math.Ceiling((double)total / pageSize);

            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null) return NotFound();
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetStatus(int id, string status)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var allowed = new[] { "Pending", "Shipped", "Completed", "Cancelled" };
            if (!allowed.Contains(status)) return BadRequest();

            order.Status = status;
            await _db.SaveChangesAsync();

            TempData["Toast"] = $"Order #{id} marked as {status}.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
