using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Entities;

namespace OnlineBookStore_Data.Data
{
    // Main EF Core DbContext for the OnlineBookStore app
    public class BookstoreContext : DbContext
    {
        public BookstoreContext(DbContextOptions<BookstoreContext> options) : base(options) { }

        public DbSet<Book> Books { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<Review> Reviews { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<User> Users => Set<User>();
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<Favorite> Favorites { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Book>(b =>
            {
                b.Property(x => x.Title).IsRequired();
                b.Property(x => x.Description).IsRequired();
            });

            modelBuilder.Entity<Author>().Property(a => a.Name).IsRequired();
            modelBuilder.Entity<Genre>().Property(g => g.Name).IsRequired();
            modelBuilder.Entity<Publisher>().Property(p => p.Name).IsRequired();
            modelBuilder.Entity<Review>().Property(r => r.Content).IsRequired();

            modelBuilder.Entity<User>(u =>
            {
                u.Property(x => x.Username).IsRequired();
                u.Property(x => x.IsAdmin).HasDefaultValue(false).IsRequired();
                u.HasIndex(x => x.Username).IsUnique();
                u.Property(x => x.AvatarPath).IsRequired(false);
            });

            modelBuilder.Entity<Review>(r =>
            {
                r.Property(x => x.Rating).IsRequired();

                r.ToTable(t => t.HasCheckConstraint(
                    "CK_Reviews_Rating_1_5",
                    "\"Rating\" >= 1 AND \"Rating\" <= 5"
                ));

                r.HasOne(x => x.Book)
                 .WithMany(b => b.Reviews)
                 .HasForeignKey(x => x.BookId)
                 .OnDelete(DeleteBehavior.Cascade);

                r.HasOne(x => x.User)
                 .WithMany(u => u.Reviews)
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                r.HasIndex(x => new { x.BookId, x.UserId }).IsUnique();
            });

            modelBuilder.Entity<Cart>(c =>
            {
                c.HasKey(x => x.CartId);
                c.Property(x => x.CartId)
                 .HasColumnType("uuid")
                 .ValueGeneratedNever();

                c.HasIndex(x => x.UserId);

                c.HasMany(x => x.Items)
                 .WithOne(i => i.Cart!)
                 .HasForeignKey(i => i.CartId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

          
            modelBuilder.Entity<CartItem>(ci =>
            {
                ci.HasKey(x => x.CartItemId);
                ci.Property(x => x.Quantity).HasDefaultValue(1);
                ci.Property(x => x.PriceAtAdd).HasColumnType("numeric(18,2)");

                ci.HasIndex(x => new { x.CartId, x.BookId }).IsUnique();

                ci.HasOne<Book>()
                  .WithMany()
                  .HasForeignKey(x => x.BookId)
                  .OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<User>(u =>
            {
                u.Property(x => x.Username).IsRequired();
                u.Property(x => x.IsAdmin).HasDefaultValue(false).IsRequired();
                u.Property(x => x.PasswordHash).IsRequired();   
                u.HasIndex(x => x.Username).IsUnique();
            }
            );

            modelBuilder.Entity<Order>(o =>
            {
                o.Property(x => x.Total).HasColumnType("numeric(18,2)");
                o.Property(x => x.Status).HasMaxLength(32);

                o.HasOne(x => x.User)
                 .WithMany()                    
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Restrict);

                o.HasMany(x => x.Items)
                 .WithOne(i => i.Order!)
                 .HasForeignKey(i => i.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(i =>
            {
                i.Property(x => x.UnitPrice).HasColumnType("numeric(18,2)");
            });

            modelBuilder.Entity<Favorite>(f =>
            {
                f.HasKey(x => new { x.UserId, x.BookId });
                f.Property(x => x.CreatedUtc).IsRequired();

                f.HasOne(x => x.User)
                 .WithMany()
                 .HasForeignKey(x => x.UserId)
                 .OnDelete(DeleteBehavior.Cascade);

                f.HasOne(x => x.Book)
                 .WithMany()
                 .HasForeignKey(x => x.BookId)
                 .OnDelete(DeleteBehavior.Cascade);

                f.HasIndex(x => x.UserId);
            });


        }
    }
}
