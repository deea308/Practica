using Microsoft.AspNetCore.Mvc;
using OnlineBookStore_Entities;
using OnlineBookstore.Service;

namespace OnlineBookstore.Controllers
{
    [Route("[controller]")]
    public class AuthorController : Controller
    {
        private readonly AuthorService _service;

        public AuthorController(AuthorService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var authors = await _service.GetAllAuthorsAsync();
            return View(authors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var author = await _service.GetAuthorAsync(id);
            if (author == null) return NotFound();
            return View(author);
        }

        [HttpGet("create")]
        public IActionResult Create() => View(new Author());

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Author model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError(nameof(model.Name), "Name is required.");

            if (!ModelState.IsValid) return View(model);

            await _service.AddAuthorAsync(model);
            TempData["Toast"] = $"Author '{model.Name}' created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var author = await _service.GetAuthorAsync(id);
            if (author == null) return NotFound();
            return View(author);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("AuthorId,Name")] Author model)
        {
            if (id != model.AuthorId) return BadRequest();

            if (string.IsNullOrWhiteSpace(model.Name))
                ModelState.AddModelError(nameof(model.Name), "Name is required.");

            if (!ModelState.IsValid) return View(model);

            await _service.UpdateAuthorAsync(model);
            TempData["Toast"] = $"Author '{model.Name}' updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _service.GetAuthorAsync(id);
            if (existing == null) return NotFound();

            await _service.DeleteAuthorAsync(id);
            TempData["Toast"] = $"Author '{existing.Name}' deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}
