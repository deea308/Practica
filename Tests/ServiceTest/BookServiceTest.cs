using FluentAssertions;
using OnlineBookstore.Services;       
using OnlineBookStore_Data.Data;
using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OnlineBookstore.Tests.ServiceTest;

public class BookServiceTest
{
    private static async Task<(Author a, Genre g, Publisher p)> SeedRefs(BookstoreContext db)
    {
        var a = new Author { Name = "J. Writer" };
        var g = new Genre { Name = "Fantasy" };
        var p = new Publisher { Name = "BigPub" };
        db.AddRange(a, g, p);
        await db.SaveChangesAsync();
        return (a, g, p);
    }

    [Fact]
    public async Task AddBook_AddsRow()
    {
        using var db = EFTestHelper.NewContext();
        var (a, g, p) = await SeedRefs(db);
        var svc = new BookService(new BookRepository(db));

        var b = new Book { Title = "Dragon Tales", Description = "Epic", Price = 25m, AuthorId = a.AuthorId, GenreId = g.GenreId, PublisherId = p.PublisherId };
        await svc.AddBookAsync(b);

        db.Books.Should().ContainSingle(x => x.Title == "Dragon Tales");
    }

    [Fact]
    public async Task GetBook_ReturnsWithDetails()
    {
        using var db = EFTestHelper.NewContext();
        var (a, g, p) = await SeedRefs(db);
        var book = new Book { Title = "B", Description = "D", Price = 10, AuthorId = a.AuthorId, GenreId = g.GenreId, PublisherId = p.PublisherId };
        db.Books.Add(book);
        await db.SaveChangesAsync();

        var svc = new BookService(new BookRepository(db));
        var loaded = await svc.GetBookAsync(book.BookId);

        loaded.Should().NotBeNull();
        loaded!.Author!.Name.Should().Be("J. Writer");
        loaded.Genre!.Name.Should().Be("Fantasy");
        loaded.Publisher!.Name.Should().Be("BigPub");
    }

    [Fact]
    public async Task UpdateBook_ChangesPersist()
    {
        using var db = EFTestHelper.NewContext();
        var (a, g, p) = await SeedRefs(db);
        var b = new Book { Title = "B", Description = "D", Price = 10, AuthorId = a.AuthorId, GenreId = g.GenreId, PublisherId = p.PublisherId };
        db.Books.Add(b);
        await db.SaveChangesAsync();

        var svc = new BookService(new BookRepository(db));
        b.Price = 19.99m;
        await svc.UpdateBookAsync(b);

        (await svc.GetBookAsync(b.BookId))!.Price.Should().Be(19.99m);
    }

    [Fact]
    public async Task DeleteBook_RemovesRow()
    {
        using var db = EFTestHelper.NewContext();
        var (a, g, p) = await SeedRefs(db);
        var b = new Book { Title = "B", Description = "D", Price = 10, AuthorId = a.AuthorId, GenreId = g.GenreId, PublisherId = p.PublisherId };
        db.Books.Add(b);
        await db.SaveChangesAsync();

        var svc = new BookService(new BookRepository(db));
        await svc.DeleteBookAsync(b.BookId);

        db.Books.Should().BeEmpty();
    }
}
