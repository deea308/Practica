using FluentAssertions;
using OnlineBookstore.Tests;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OnlineBookstore_Tests.Tests.RepositoryTest;

public class GenreRepositoryTest
{
    private static async Task SeedAsync(BookstoreContext db)
    {
        var a = new Author { Name = "A" };
        var p = new Publisher { Name = "P" };
        var g1 = new Genre { Name = "Fantasy" };
        var g2 = new Genre { Name = "Horror" };

        db.AddRange(
            new Book { Title = "Dragons", Description = "D", Price = 10, Author = a, Genre = g1, Publisher = p },
            new Book { Title = "Knights", Description = "D", Price = 12, Author = a, Genre = g1, Publisher = p },
            new Book { Title = "Screams", Description = "D", Price = 9, Author = a, Genre = g2, Publisher = p }
        );
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_includes_Books()
    {
        using var db = EFTestHelper.NewContext();
        await SeedAsync(db);
        var repo = new GenreRepository(db);

        var genres = (await repo.GetAllAsync()).ToList();

        genres.Should().HaveCount(2);
        genres.Select(g => g.Books.Count).Sum().Should().Be(3);
    }

    [Fact]
    public async Task SearchPagedAsync_by_name_or_book()
    {
        using var db = EFTestHelper.NewContext();
        await SeedAsync(db);
        var repo = new GenreRepository(db);

        (await repo.SearchPagedAsync("Horror", 1, 10)).Total.Should().Be(1);
        (await repo.SearchPagedAsync("Dragons", 1, 10)).Total.Should().Be(1);
    }
}
