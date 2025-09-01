using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using OnlineBookstore.Authorization;
using OnlineBookStore_Entities;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace OnlineBookstore.Tests.AuthorizationTest;

public class AdminHandlerTests
{
    private static ClaimsPrincipal PrincipalWithId(int? id)
    {
        var claims = new List<Claim>();
        if (id.HasValue)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, id.Value.ToString()));
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task Grants_requirement_when_user_is_admin()
    {
        using var db = new SqliteInMemory();

        var admin = new User { Username = "admin", Email = "a@x", PasswordHash = "ph", IsAdmin = true };
        db.Context.Users.Add(admin);
        await db.Context.SaveChangesAsync();

        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new AdminHandler(db.Context, cache);
        var req = new AdminRequirement();
        var user = PrincipalWithId(admin.UserId);
        var ctx = new AuthorizationHandlerContext(new[] { req }, user, resource: null);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task Does_not_grant_when_user_is_not_admin()
    {
        using var db = new SqliteInMemory();

        var u = new User { Username = "user", Email = "u@x", PasswordHash = "ph", IsAdmin = false };
        db.Context.Users.Add(u);
        await db.Context.SaveChangesAsync();

        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new AdminHandler(db.Context, cache);
        var req = new AdminRequirement();
        var user = PrincipalWithId(u.UserId);
        var ctx = new AuthorizationHandlerContext(new[] { req }, user, resource: null);

        await handler.HandleAsync(ctx);

        ctx.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task Ignores_when_nameidentifier_claim_is_missing_or_invalid()
    {
        using var db = new SqliteInMemory();

        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new AdminHandler(db.Context, cache);
        var req = new AdminRequirement();

        var noClaim = new ClaimsPrincipal(new ClaimsIdentity()); 
        var ctx1 = new AuthorizationHandlerContext(new[] { req }, noClaim, resource: null);
        await handler.HandleAsync(ctx1);
        ctx1.HasSucceeded.Should().BeFalse();

        var badClaim = new ClaimsPrincipal(
            new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, "abc") }, "TestAuth"));
        var ctx2 = new AuthorizationHandlerContext(new[] { req }, badClaim, resource: null);
        await handler.HandleAsync(ctx2);
        ctx2.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task Uses_cache_for_subsequent_checks()
    {
        using var db = new SqliteInMemory();

        var admin = new User { Username = "admin", Email = "a@x", PasswordHash = "ph", IsAdmin = true };
        db.Context.Users.Add(admin);
        await db.Context.SaveChangesAsync();

        var cache = new MemoryCache(new MemoryCacheOptions());
        var handler = new AdminHandler(db.Context, cache);
        var req = new AdminRequirement();
        var user = PrincipalWithId(admin.UserId);

        var ctx1 = new AuthorizationHandlerContext(new[] { req }, user, resource: null);
        await handler.HandleAsync(ctx1);
        ctx1.HasSucceeded.Should().BeTrue();

        admin.IsAdmin = false;
        await db.Context.SaveChangesAsync();

        var ctx2 = new AuthorizationHandlerContext(new[] { req }, user, resource: null);
        await handler.HandleAsync(ctx2);
        ctx2.HasSucceeded.Should().BeTrue(); 
    }
}

