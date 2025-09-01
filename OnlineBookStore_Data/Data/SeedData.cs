using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Entities;

public static class SeedData
{
    // One-time startup seeding: ensures basic data exists and admin user has a password
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BookstoreContext>();
        await context.Database.MigrateAsync();

        var hasher = new PasswordHasher<User>();

        var admin = await context.Users.FirstOrDefaultAsync(u => u.Username == "Andreea");
        if (admin == null)
        {
            admin = new User
            {
                Username = "Andreea",
                Email = "andreea@site.tld",
                IsAdmin = true
            };
            admin.PasswordHash = hasher.HashPassword(admin, "Andreea3");
            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
        else if (string.IsNullOrEmpty(admin.PasswordHash))
        {
            admin.PasswordHash = hasher.HashPassword(admin, "Andreea3");
            await context.SaveChangesAsync();
        }

        var reader = await context.Users.FirstOrDefaultAsync(u => u.Username == "Alessia");
        if (reader == null)
        {
            reader = new User
            {
                Username = "Alessia",
                Email = "alessia@site.tld",
                IsAdmin = false
            };
            reader.PasswordHash = hasher.HashPassword(reader, "Alessia3");
            context.Users.Add(reader);
            await context.SaveChangesAsync();
        }
        else if (string.IsNullOrEmpty(reader.PasswordHash))
        {
            reader.PasswordHash = hasher.HashPassword(reader, "Alessia3");
            await context.SaveChangesAsync();
        }

        var author = await context.Authors.FirstOrDefaultAsync(a => a.Name == "J K Rowling");
        if (author == null)
        {
            author = new Author { Name = "J K Rowling" };
            context.Authors.Add(author);
            await context.SaveChangesAsync();
        }

        var genre = await context.Genres.FirstOrDefaultAsync(g => g.Name == "Fantasy");
        if (genre == null)
        {
            genre = new Genre { Name = "Fantasy" };
            context.Genres.Add(genre);
            await context.SaveChangesAsync();
        }

        var publisher = await context.Publishers.FirstOrDefaultAsync(p => p.Name == "Arthur");
        if (publisher == null)
        {
            publisher = new Publisher { Name = "Arthur" };
            context.Publishers.Add(publisher);
            await context.SaveChangesAsync();
        }

        var book = await context.Books.FirstOrDefaultAsync(b => b.Title == "Harry Potter");
        if (book == null)
        {
            book = new Book
            {
                Title = "Harry Potter",
                Price = 50m,
                Description = "A fantasy new book",
                AuthorId = author.AuthorId,
                GenreId = genre.GenreId,
                PublisherId = publisher.PublisherId
            };
            context.Books.Add(book);
            await context.SaveChangesAsync();
        }

        var existingReview = await context.Reviews
            .FirstOrDefaultAsync(r => r.BookId == book.BookId && r.UserId == admin.UserId);

        if (existingReview == null)
        {
            context.Reviews.Add(new Review
            {
                Content = "A captivating book",
                Rating = 5,
                BookId = book.BookId,
                UserId = admin.UserId
            });
            await context.SaveChangesAsync();
        }
    }
}
