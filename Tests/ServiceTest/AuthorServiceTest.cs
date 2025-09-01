using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OnlineBookstore.Service;
using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using Xunit;

namespace OnlineBookstore.Tests.ServiceTest;

public class AuthorServiceTest
{
    [Fact]
    public async Task AddAuthor_AddsRow()
    {
        using var db = EFTestHelper.NewContext();
        var svc = new AuthorService(new AuthorRepository(db));

        await svc.AddAuthorAsync(new Author { Name = "Jane Doe" });

        db.Authors.Should().ContainSingle(a => a.Name == "Jane Doe");
    }

    [Fact]
    public async Task GetAuthor_ReturnsAuthorWithBooks()
    {
        using var db = EFTestHelper.NewContext();
        var author = new Author { Name = "A" };
        db.Authors.Add(author);
        await db.SaveChangesAsync();

        var svc = new AuthorService(new AuthorRepository(db));
        var loaded = await svc.GetAuthorAsync(author.AuthorId);

        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("A");
        loaded.Books.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAuthor_ChangesPersist()
    {
        using var db = EFTestHelper.NewContext();
        var author = new Author { Name = "Old" };
        db.Authors.Add(author);
        await db.SaveChangesAsync();

        var svc = new AuthorService(new AuthorRepository(db));
        author.Name = "New";
        await svc.UpdateAuthorAsync(author);

        (await svc.GetAuthorAsync(author.AuthorId))!.Name.Should().Be("New");
    }

    [Fact]
    public async Task DeleteAuthor_RemovesRow()
    {
        using var db = EFTestHelper.NewContext();
        var a = new Author { Name = "X" };
        db.Authors.Add(a);
        await db.SaveChangesAsync();

        var svc = new AuthorService(new AuthorRepository(db));
        await svc.DeleteAuthorAsync(a.AuthorId);

        db.Authors.Should().BeEmpty();
    }
}
