using FluentAssertions;
using OnlineBookstore.Tests;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OnlineBookstore_Tests.Tests.RepositoryTest;

public class AuthorRepositoryTest
{
    private static async Task SeedAsync(BookstoreContext db)
    {
        var a1 = new Author { Name = "A-One" };
        var a2 = new Author { Name = "A-Two" };
        var g = new Genre { Name = "Romance" };
        var p = new Publisher { Name = "Pub" };

        db.AddRange(
            new Book { Title = "First Book", Description = "D", Price = 10, Author = a1, Genre = g, Publisher = p },
            new Book { Title = "Second Book", Description = "D", Price = 12, Author = a1, Genre = g, Publisher = p },
            new Book { Title = "Third", Description = "D", Price = 9, Author = a2, Genre = g, Publisher = p }
        );
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_includes_Books()
    {
        using var db = EFTestHelper.NewContext();
        await SeedAsync(db);
        var repo = new AuthorRepository(db);

        var authors = (await repo.GetAllAsync()).ToList();

        authors.Should().HaveCount(2);
        authors.Select(a => a.Books.Count).Sum().Should().Be(3);
    }

    [Fact]
    public async Task GetByIdAsync_includes_Books()
    {
        using var db = EFTestHelper.NewContext();
        await SeedAsync(db);
        var repo = new AuthorRepository(db);

        var id = db.Authors.Single(a => a.Name == "A-One").AuthorId;
        var a1 = await repo.GetByIdAsync(id);

        a1.Should().NotBeNull();
        a1!.Books.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchPagedAsync_by_author_or_book_title()
    {
        using var db = EFTestHelper.NewContext();
        await SeedAsync(db);
        var repo = new AuthorRepository(db);

        var (byAuthor, total1) = await repo.SearchPagedAsync("A-One", 1, 10);
        total1.Should().Be(1);
        byAuthor.Single().Name.Should().Be("A-One");

        var (byBook, total2) = await repo.SearchPagedAsync("Third", 1, 10);
        total2.Should().Be(1);
        byBook.Single().Name.Should().Be("A-Two");
    }
}
