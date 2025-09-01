using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Entities;
using OnlineBookstore.Service;                 
using OnlineBookstore.ViewModels;             
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBookstore.Controllers
{
    [Route("[controller]")]
    // Admin listing and management of publishers
    public class PublisherController : Controller
    {
        private readonly PublisherService _service;   
        private readonly BookstoreContext _db;        

        public PublisherController(PublisherService service, BookstoreContext db)
        {
            _service = service;
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var items = await _db.Publishers
                .GroupJoin(_db.Books,
                    p => p.PublisherId,
                    b => b.PublisherId,
                    (p, books) => new PublisherListItemVm
                    {
                        PublisherId = p.PublisherId,
                        Name = p.Name,
                        BookCount = books.Count()
                    })
                .OrderBy(x => x.Name)
                .ToListAsync();

            return View(items); 
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var pub = await _db.Publishers
                .Where(p => p.PublisherId == id)
                .Select(p => new { p.PublisherId, p.Name })
                .FirstOrDefaultAsync();

            if (pub == null) return NotFound();

            var books = await _db.Books
                .Where(b => b.PublisherId == id)
                .OrderBy(b => b.Title)
                .Select(b => new BookBriefVm
                {
                    BookId = b.BookId,
                    Title = b.Title,
                    Price = b.Price
                })
                .ToListAsync();

            var vm = new PublisherDetailsVm
            {
                PublisherId = pub.PublisherId,
                Name = pub.Name,
                BookCount = books.Count,
                Books = books
            };

            return View(vm); 
        }

        [HttpGet("create")]
        public IActionResult Create() => View(new Publisher());

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Publisher publisher)
        {
            if (string.IsNullOrWhiteSpace(publisher.Name))
                ModelState.AddModelError(nameof(publisher.Name), "Name is required.");

            if (!ModelState.IsValid) return View(publisher);

            await _service.AddPublisherAsync(publisher);
            TempData["Toast"] = "Publisher created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var publisher = await _service.GetPublisherAsync(id);
            if (publisher == null) return NotFound();
            return View(publisher); 
        }

        [HttpPost("edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PublisherId,Name")] Publisher publisher)
        {
            if (id != publisher.PublisherId) return BadRequest();

            if (string.IsNullOrWhiteSpace(publisher.Name))
                ModelState.AddModelError(nameof(publisher.Name), "Name is required.");

            if (!ModelState.IsValid) return View(publisher);

            await _service.UpdatePublisherAsync(publisher);
            TempData["Toast"] = "Publisher updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("delete/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeletePublisherAsync(id);
            TempData["Toast"] = "Publisher deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
