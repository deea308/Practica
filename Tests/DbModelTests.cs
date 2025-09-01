using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Entities;
using Xunit;

namespace OnlineBookstore.Tests;

public class DbModelTests
{
    [Fact]
    public async Task Users_Unique_Username_And_Defaults_Work()
    {
        using var db = new SqliteInMemory();

        var u1 = new User { Username = "andreea", Email = "a@site.tld", PasswordHash = "x" };
        var u2 = new User { Username = "andreea", Email = "b@site.tld", PasswordHash = "y" }; 

        db.Context.Users.Add(u1);
        await db.Context.SaveChangesAsync();

        db.Context.Users.Add(u2);
        Func<Task> act = async () => await db.Context.SaveChangesAsync();

        await act.Should().ThrowAsync<DbUpdateException>(); 

        var saved = await db.Context.Users.SingleAsync(u => u.Username == "andreea");
        saved.IsAdmin.Should().BeFalse();
    }

    [Fact]
    public async Task Reviews_Unique_Per_User_Per_Book_Is_Enforced()
    {
        using var db = new SqliteInMemory();

        var a = new Author { Name = "A" };
        var g = new Genre { Name = "G" };
        var p = new Publisher { Name = "P" };
        var b = new Book { Title = "T", Description = "D", Price = 10, Author = a, Genre = g, Publisher = p };
        var u = new User { Username = "u1", Email = "u1@x", PasswordHash = "ph" };

        db.Context.AddRange(a, g, p, b, u);
        await db.Context.SaveChangesAsync();

        db.Context.Reviews.Add(new Review { BookId = b.BookId, UserId = u.UserId, Rating = 3, Content = "first" });
        await db.Context.SaveChangesAsync();

        Func<Task> duplicate = async () =>
        {
            db.Context.Reviews.Add(new Review { BookId = b.BookId, UserId = u.UserId, Rating = 4, Content = "dup" });
            await db.Context.SaveChangesAsync();
        };
        await duplicate.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Delete_User_Cascades_To_Reviews()
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

        db.Context.Users.Remove(u);
        await db.Context.SaveChangesAsync();

        (await db.Context.Reviews.CountAsync()).Should().Be(0);
    }

    [Fact]
    public async Task CartItem_Default_Quantity_Is_One_When_Not_Set()
    {
        using var db = new SqliteInMemory();

        var a = new Author { Name = "A" };
        var g = new Genre { Name = "G" };
        var p = new Publisher { Name = "P" };
        var b = new Book { Title = "T", Description = "D", Price = 10, Author = a, Genre = g, Publisher = p };
        var cart = new Cart { CartId = Guid.NewGuid() };

        db.Context.AddRange(a, g, p, b, cart);
        await db.Context.SaveChangesAsync();

        db.Context.CartItems.Add(new CartItem
        {
            CartId = cart.CartId,
            BookId = b.BookId,
            PriceAtAdd = 1.23m
        });
        await db.Context.SaveChangesAsync();

        var ci = await db.Context.CartItems.SingleAsync();
        ci.Quantity.Should().Be(1);
    }
}
