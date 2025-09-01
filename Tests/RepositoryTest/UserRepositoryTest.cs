using FluentAssertions;
using OnlineBookstore.Tests;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OnlineBookstore_Tests.Tests.RepositoryTest;

public class UserRepositoryTest
{
    private static async Task SeedAsync(BookstoreContext db)
    {
        var a = new Author { Name = "A" };
        var g = new Genre { Name = "G" };
        var p = new Publisher { Name = "P" };
        var b = new Book { Title = "B", Description = "D", Price = 10, Author = a, Genre = g, Publisher = p };
        var u1 = new User { Username = "andreea", Email = "a@x", PasswordHash = "ph" };
        var u2 = new User { Username = "alex", Email = "b@x", PasswordHash = "ph" };

        db.AddRange(
            new Review { Book = b, User = u1, Rating = 5, Content = "Love" },
            new Review { Book = b, User = u2, Rating = 4, Content = "Good" }
        );
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_includes_Reviews()
    {
        using var db = EFTestHelper.NewContext();
        await SeedAsync(db);
        var repo = new UserRepository(db);

        var users = (await repo.GetAllAsync()).ToList();

        users.Should().HaveCount(2);
        users.Select(u => u.Reviews!.Count).Sum().Should().Be(2);
    }

    [Fact]
    public async Task SearchPagedAsync_by_username_email_or_review()
    {
        using var db = EFTestHelper.NewContext();
        await SeedAsync(db);
        var repo = new UserRepository(db);

        (await repo.SearchPagedAsync("alex", 1, 10)).Total.Should().Be(1);
        (await repo.SearchPagedAsync("@x", 1, 10)).Total.Should().Be(2);
        (await repo.SearchPagedAsync("Love", 1, 10)).Total.Should().Be(1);
    }
}
