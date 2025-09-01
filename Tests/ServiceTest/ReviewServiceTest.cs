using FluentAssertions;
using OnlineBookstore.Service;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using System.Threading.Tasks;
using Xunit;

namespace OnlineBookstore.Tests.ServiceTest;

public class ReviewServiceTest
{
    private static async Task<(Book b, User u)> Seed(BookstoreContext db)
    {
        var a = new Author { Name = "A" };
        var g = new Genre { Name = "G" };
        var p = new Publisher { Name = "P" };
        var b = new Book { Title = "B", Description = "D", Price = 10, Author = a, Genre = g, Publisher = p };
        var u = new User { Username = "alex", Email = "a@x", PasswordHash = "ph" };
        db.AddRange(b, u);
        await db.SaveChangesAsync();
        return (b, u);
    }

    [Fact]
    public async Task AddReview_AddsRow()
    {
        using var db = EFTestHelper.NewContext();
        var (b, u) = await Seed(db);
        var svc = new ReviewService(new ReviewRepository(db));

        await svc.AddReviewAsync(new Review { BookId = b.BookId, UserId = u.UserId, Rating = 5, Content = "Great!" });

        db.Reviews.Should().ContainSingle(r => r.BookId == b.BookId && r.UserId == u.UserId && r.Rating == 5);
    }

    [Fact]
    public async Task GetReview_ReturnsWithBookAndUser()
    {
        using var db = EFTestHelper.NewContext();
        var (b, u) = await Seed(db);
        var r = new Review { BookId = b.BookId, UserId = u.UserId, Rating = 4, Content = "Ok" };
        db.Reviews.Add(r);
        await db.SaveChangesAsync();

        var svc = new ReviewService(new ReviewRepository(db));
        var loaded = await svc.GetReviewAsync(r.ReviewId);

        loaded.Should().NotBeNull();
        loaded!.Book!.Title.Should().Be("B");
        loaded.User!.Username.Should().Be("alex");
    }

    [Fact]
    public async Task UpdateReview_ChangesPersist()
    {
        using var db = EFTestHelper.NewContext();
        var (b, u) = await Seed(db);
        var r = new Review { BookId = b.BookId, UserId = u.UserId, Rating = 3, Content = "meh" };
        db.Reviews.Add(r);
        await db.SaveChangesAsync();

        var svc = new ReviewService(new ReviewRepository(db));
        r.Content = "better";
        r.Rating = 4;
        await svc.UpdateReviewAsync(r);

        var after = await svc.GetReviewAsync(r.ReviewId);
        after!.Content.Should().Be("better");
        after.Rating.Should().Be(4);
    }

    [Fact]
    public async Task DeleteReview_RemovesRow()
    {
        using var db = EFTestHelper.NewContext();
        var (b, u) = await Seed(db);
        var r = new Review { BookId = b.BookId, UserId = u.UserId, Rating = 5, Content = "nice" };
        db.Reviews.Add(r);
        await db.SaveChangesAsync();

        var svc = new ReviewService(new ReviewRepository(db));
        await svc.DeleteReviewAsync(r.ReviewId);

        db.Reviews.Should().BeEmpty();
    }
}

