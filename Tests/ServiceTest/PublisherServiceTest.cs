using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OnlineBookstore.Service;
using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using Xunit;

namespace OnlineBookstore.Tests.ServiceTest;

public class PublisherServiceTest
{
    [Fact]
    public async Task AddPublisher_AddsRow()
    {
        using var db = EFTestHelper.NewContext();
        var svc = new PublisherService(new PublisherRepository(db));

        await svc.AddPublisherAsync(new Publisher { Name = "Alpha" });

        db.Publishers.Should().ContainSingle(p => p.Name == "Alpha");
    }

    [Fact]
    public async Task GetPublisher_ReturnsRow()
    {
        using var db = EFTestHelper.NewContext();
        var p = new Publisher { Name = "Beta" };
        db.Publishers.Add(p);
        await db.SaveChangesAsync();

        var svc = new PublisherService(new PublisherRepository(db));
        var loaded = await svc.GetPublisherAsync(p.PublisherId);

        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("Beta");
    }

    [Fact]
    public async Task UpdatePublisher_ChangesPersist()
    {
        using var db = EFTestHelper.NewContext();
        var p = new Publisher { Name = "Old" };
        db.Publishers.Add(p);
        await db.SaveChangesAsync();

        var svc = new PublisherService(new PublisherRepository(db));
        p.Name = "New";
        await svc.UpdatePublisherAsync(p);

        (await svc.GetPublisherAsync(p.PublisherId))!.Name.Should().Be("New");
    }

    [Fact]
    public async Task DeletePublisher_RemovesRow()
    {
        using var db = EFTestHelper.NewContext();
        var p = new Publisher { Name = "X" };
        db.Publishers.Add(p);
        await db.SaveChangesAsync();

        var svc = new PublisherService(new PublisherRepository(db));
        await svc.DeletePublisherAsync(p.PublisherId);

        db.Publishers.Should().BeEmpty();
    }
}
