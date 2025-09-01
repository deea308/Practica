using Microsoft.AspNetCore.Mvc;
using OnlineBookstore.Service;
using OnlineBookStore_Entities;

namespace OnlineBookstore.Controllers
{
    [Route("[controller]")]
    public class GenreController : Controller
    {
        private readonly GenreService _service;
        public GenreController(GenreService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var genres = await _service.GetAllGenresAsync();
            return View(genres);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var genre = await _service.GetGenreAsync(id);
            if (genre == null) return NotFound();
            return View(genre);
        }

        [HttpGet("create")]
        public IActionResult Create() => View(new Genre());

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] Genre genre)
        {
            if (string.IsNullOrWhiteSpace(genre.Name))
                ModelState.AddModelError(nameof(genre.Name), "Name is required.");

            if (!ModelState.IsValid) return View(genre);

            await _service.AddGenreAsync(genre);
            TempData["Toast"] = "Genre created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var genre = await _service.GetGenreAsync(id);
            if (genre == null) return NotFound();
            return View(genre);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GenreId,Name")] Genre genre)
        {
            if (id != genre.GenreId) return BadRequest();
            if (string.IsNullOrWhiteSpace(genre.Name))
                ModelState.AddModelError(nameof(genre.Name), "Name is required.");

            if (!ModelState.IsValid) return View(genre);

            await _service.UpdateGenreAsync(genre);
            TempData["Toast"] = "Genre updated.";
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteGenreAsync(id);
            TempData["Toast"] = "Genre deleted.";
            return RedirectToAction(nameof(Index));
        }
    }
}



