using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using OnlineBookstore.Service;
using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using Xunit;

namespace OnlineBookstore.Tests.ServiceTest;

public class UserServiceTest
{
    [Fact]
    public async Task AddUser_AddsRow()
    {
        using var db = EFTestHelper.NewContext();
        var svc = new UserService(new UserRepository(db));

        await svc.AddUserAsync(new User { Username = "denisa", Email = "d@x", PasswordHash = "ph" });

        db.Users.Should().ContainSingle(u => u.Username == "denisa");
    }

    [Fact]
    public async Task GetUser_ReturnsWithReviewsCollection()
    {
        using var db = EFTestHelper.NewContext();
        var u = new User { Username = "alessia", Email = "a@x", PasswordHash = "ph" };
        db.Users.Add(u);
        await db.SaveChangesAsync();

        var svc = new UserService(new UserRepository(db));
        var loaded = await svc.GetUserAsync(u.UserId);

        loaded.Should().NotBeNull();
        loaded!.Username.Should().Be("alessia");
        loaded.Reviews.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateUser_ChangesPersist()
    {
        using var db = EFTestHelper.NewContext();
        var u = new User { Username = "alex", Email = "a@x", PasswordHash = "ph" };
        db.Users.Add(u);
        await db.SaveChangesAsync();

        var svc = new UserService(new UserRepository(db));
        u.Email = "alex@site.tld";
        await svc.UpdateUserAsync(u);

        (await svc.GetUserAsync(u.UserId))!.Email.Should().Be("alex@site.tld");
    }

    [Fact]
    public async Task DeleteUser_RemovesRow()
    {
        using var db = EFTestHelper.NewContext();
        var u = new User { Username = "temp", Email = "t@x", PasswordHash = "ph" };
        db.Users.Add(u);
        await db.SaveChangesAsync();

        var svc = new UserService(new UserRepository(db));
        await svc.DeleteUserAsync(u.UserId);

        db.Users.Should().BeEmpty();
    }
}
