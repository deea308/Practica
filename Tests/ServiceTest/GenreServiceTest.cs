using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OnlineBookstore.Service;
using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using Xunit;

namespace OnlineBookstore.Tests.ServiceTest;

public class GenreServiceTest
{
    [Fact]
    public async Task AddGenre_AddsRow()
    {
        using var db = EFTestHelper.NewContext();
        var svc = new GenreService(new GenreRepository(db));

        await svc.AddGenreAsync(new Genre { Name = "Romance" });

        db.Genres.Should().ContainSingle(g => g.Name == "Romance");
    }

    [Fact]
    public async Task GetGenre_ReturnsWithBooks()
    {
        using var db = EFTestHelper.NewContext();
        var genre = new Genre { Name = "SciFi" };
        db.Genres.Add(genre);
        await db.SaveChangesAsync();

        var svc = new GenreService(new GenreRepository(db));
        var loaded = await svc.GetGenreAsync(genre.GenreId);

        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("SciFi");
    }

    [Fact]
    public async Task UpdateGenre_ChangesPersist()
    {
        using var db = EFTestHelper.NewContext();
        var g = new Genre { Name = "Old" };
        db.Genres.Add(g);
        await db.SaveChangesAsync();

        var svc = new GenreService(new GenreRepository(db));
        g.Name = "New";
        await svc.UpdateGenreAsync(g);

        (await svc.GetGenreAsync(g.GenreId))!.Name.Should().Be("New");
    }

    [Fact]
    public async Task DeleteGenre_RemovesRow()
    {
        using var db = EFTestHelper.NewContext();
        var g = new Genre { Name = "X" };
        db.Genres.Add(g);
        await db.SaveChangesAsync();

        var svc = new GenreService(new GenreRepository(db));
        await svc.DeleteGenreAsync(g.GenreId);

        db.Genres.Should().BeEmpty();
    }
}
