using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineBookstore.Service;
using OnlineBookstore.Services;
using OnlineBookStore_Entities;

namespace OnlineBookstore.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    // Admin CRUD for Books, including cover uploads and related entities
    public class BookController : Controller
    {
        private readonly BookService _bookService;
        private readonly AuthorService _authorService;
        private readonly GenreService _genreService;
        private readonly PublisherService _publisherService;
        private readonly IWebHostEnvironment _env;

        public BookController(
            BookService bookService,
            AuthorService authorService,
            GenreService genreService,
            PublisherService publisherService,
            IWebHostEnvironment env)
        {
            _bookService = bookService;
            _authorService = authorService;
            _genreService = genreService;
            _publisherService = publisherService;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var books = await _bookService.GetAllBooksAsync();
            return View(books);
        }

        [HttpGet]
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            string title, string description, decimal price,
            string authorName, string genreName, string publisherName,
            IFormFile? cover)
        {
            if (string.IsNullOrWhiteSpace(title) ||
                string.IsNullOrWhiteSpace(authorName) ||
                string.IsNullOrWhiteSpace(genreName) ||
                string.IsNullOrWhiteSpace(publisherName))
            {
                ModelState.AddModelError("", "Please fill all required fields.");
                return View();
            }

            var author = (await _authorService.GetAllAuthorsAsync())
                         .FirstOrDefault(a => a.Name.Equals(authorName, StringComparison.OrdinalIgnoreCase))
                         ?? await AddAuthor(authorName);

            var genre = (await _genreService.GetAllGenresAsync())
                        .FirstOrDefault(g => g.Name.Equals(genreName, StringComparison.OrdinalIgnoreCase))
                        ?? await AddGenre(genreName);

            var publisher = (await _publisherService.GetAllPublishersAsync())
                            .FirstOrDefault(p => p.Name.Equals(publisherName, StringComparison.OrdinalIgnoreCase))
                            ?? await AddPublisher(publisherName);

            var book = new Book
            {
                Title = title,
                Description = description,
                Price = price,
                AuthorId = author.AuthorId,
                GenreId = genre.GenreId,
                PublisherId = publisher.PublisherId
            };

            if (cover is { Length: > 0 })
            {
                var rel = await SaveCoverAsync(cover);
                if (rel == null)
                {
                    ModelState.AddModelError("", "Only .jpg/.jpeg/.png/.webp up to 2 MB.");
                    return View();
                }
                book.CoverImagePath = rel;
            }

            await _bookService.AddBookAsync(book);
            return RedirectToAction(nameof(Index));

            async Task<Author> AddAuthor(string name) { var a = new Author { Name = name.Trim() }; await _authorService.AddAuthorAsync(a); return a; }
            async Task<Genre> AddGenre(string name) { var g = new Genre { Name = name.Trim() }; await _genreService.AddGenreAsync(g); return g; }
            async Task<Publisher> AddPublisher(string name) { var p = new Publisher { Name = name.Trim() }; await _publisherService.AddPublisherAsync(p); return p; }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var book = await _bookService.GetBookAsync(id);
            if (book == null) return NotFound();

            var authors = await _authorService.GetAllAuthorsAsync();
            ViewBag.Authors = new SelectList(authors, "AuthorId", "Name", book.AuthorId);
            return View(book);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Book model, string? PublisherName, IFormFile? cover)
        {
            if (!ModelState.IsValid)
            {
                var authors = await _authorService.GetAllAuthorsAsync();
                ViewBag.Authors = new SelectList(authors, "AuthorId", "Name", model.AuthorId);
                return View(model);
            }

            var book = await _bookService.GetBookAsync(model.BookId);
            if (book == null) return NotFound();

            book.Title = (model.Title ?? "").Trim();   
            book.Description = model.Description;
            book.Price = model.Price;
            book.AuthorId = model.AuthorId;

            if (!string.IsNullOrWhiteSpace(PublisherName))
            {
                var name = PublisherName.Trim();
                var existing = (await _publisherService.GetAllPublishersAsync())
                               .FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    existing = new Publisher { Name = name };
                    await _publisherService.AddPublisherAsync(existing);
                }
                book.PublisherId = existing.PublisherId;
            }

            if (cover is { Length: > 0 })
            {
                var rel = await SaveCoverAsync(cover);
                if (rel == null)
                {
                    var authors = await _authorService.GetAllAuthorsAsync();
                    ViewBag.Authors = new SelectList(authors, "AuthorId", "Name", model.AuthorId);
                    ModelState.AddModelError("", "Only .jpg/.jpeg/.png/.webp up to 2 MB.");
                    return View(model);
                }

                if (!string.IsNullOrEmpty(book.CoverImagePath))
                {
                    var oldFull = Path.Combine(_env.WebRootPath,
                        book.CoverImagePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(oldFull)) System.IO.File.Delete(oldFull);
                }

                book.CoverImagePath = rel;
            }

            await _bookService.UpdateBookAsync(book);
            TempData["Toast"] = "Saved.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> SaveCoverAsync(IFormFile file)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowed.Contains(ext) || file.Length > 2 * 1024 * 1024)
                return null;

            var folder = Path.Combine(_env.WebRootPath, "uploads", "covers");
            Directory.CreateDirectory(folder);

            var name = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(folder, name);

            using (var stream = System.IO.File.Create(fullPath))
                await file.CopyToAsync(stream);

            return $"/uploads/covers/{name}";
        }
    }
}
