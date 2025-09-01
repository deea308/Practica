using FluentAssertions;
using OnlineBookstore.Tests;
using OnlineBookStore_Data.Data;
using OnlineBookStore_Data.Repository;
using OnlineBookStore_Entities;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace OnlineBookstore_Tests.Tests.RepositoryTest;

public class PublisherRepositoryTest
{
    private static async Task SeedAsync(BookstoreContext db)
    {
        db.Publishers.AddRange(
            new Publisher { Name = "Alpha" },
            new Publisher { Name = "Beta" },
            new Publisher { Name = "Gamma" }
        );
        await db.SaveChangesAsync();
    }

}
