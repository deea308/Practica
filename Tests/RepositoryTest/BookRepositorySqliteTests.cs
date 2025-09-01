using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OnlineBookstore.Tests;
using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using Xunit;

namespace OnlineBookstore_Tests.Tests.RepositoryTest;

public class BookRepositorySqliteTests
{
    [Fact]
    public async Task GetByIdAsync_Includes_Author_Genre_Publisher_Reviews_User()
    {
        using var db = new SqliteInMemory();

        var a = new Author { Name = "A" };
        var g = new Genre { Name = "G" };
        var p = new Publisher { Name = "P" };
        var b = new Book { Title = "T", Description = "D", Price = 10, Author = a, Genre = g, Publisher = p };
        var u = new User { Username = "u1", Email = "u1@x", PasswordHash = "ph" };

        db.Context.AddRange(a, g, p, b, u);
        await db.Context.SaveChangesAsync();

        db.Context.Reviews.Add(new Review { BookId = b.BookId, UserId = u.UserId, Rating = 5, Content = "hi" });
        await db.Context.SaveChangesAsync();

        var repo = new BookRepository(db.Context);
        var got = await repo.GetByIdAsync(b.BookId);

        got.Should().NotBeNull();
        got!.Author.Should().NotBeNull();
        got.Genre.Should().NotBeNull();
        got.Publisher.Should().NotBeNull();
        got.Reviews.Should().HaveCount(1);
        got.Reviews.First().User.Should().NotBeNull();
    }
}
