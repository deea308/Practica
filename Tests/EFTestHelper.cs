
using System; 
using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Data.Data;

namespace OnlineBookstore.Tests;

internal static class EFTestHelper
{
    public static BookstoreContext NewContext(string? name = null)
    {
        var options = new DbContextOptionsBuilder<BookstoreContext>()
            .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString()) 
            .EnableSensitiveDataLogging()
            .Options;

        return new BookstoreContext(options);
    }
}
