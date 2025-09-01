using FluentAssertions;
using OnlineBookstore.Tests;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OnlineBookstore_Tests.Tests.RepositoryTest;

public class ReviewRepositoryTest
{
    private static async Task SeedAsync(BookstoreContext db)
    {
        var a = new Author { Name = "A" };
        var g = new Genre { Name = "G" };
        var p = new Publisher { Name = "P" };
        var b1 = new Book { Title = "B1", Description = "D", Price = 10, Author = a, Genre = g, Publisher = p };
        var b2 = new Book { Title = "B2", Description = "D", Price = 11, Author = a, Genre = g, Publisher = p };
        var u1 = new User { Username = "andreea", Email = "a@x", PasswordHash = "ph" };
        var u2 = new User { Username = "alex", Email = "b@x", PasswordHash = "ph" };

        db.AddRange(
            new Review { Book = b1, User = u1, Rating = 5, Content = "Great" },
            new Review { Book = b2, User = u2, Rating = 4, Content = "Nice" }
        );
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_includes_Book_and_User()
    {
        using var db = EFTestHelper.NewContext();
        await SeedAsync(db);
        var repo = new ReviewRepository(db);

        var all = (await repo.GetAllAsync()).ToList();

        all.Should().HaveCount(2);
        all.All(r => r.Book != null && r.User != null).Should().BeTrue();
    }

    [Fact]
    public async Task SearchPagedAsync_by_book_or_user()
    {
        using var db = EFTestHelper.NewContext();
        await SeedAsync(db);
        var repo = new ReviewRepository(db);

        (await repo.SearchPagedAsync("B1", 1, 10)).Total.Should().Be(1);
        (await repo.SearchPagedAsync("alex", 1, 10)).Total.Should().Be(1);
    }
}
