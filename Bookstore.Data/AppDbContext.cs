using Microsoft.EntityFrameworkCore;
using Bookstore.Data.Entities;

namespace Bookstore.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; } = null!;
        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<PasswordReset> PasswordResets { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!; // New Cart DbSet

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // RefreshToken Configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.Property(rt => rt.Expires).IsRequired();
            });

            // Book Configuration
            modelBuilder.Entity<Book>(entity =>
            {
                // Decimal Precision Settings
                entity.Property(b => b.Price)
                    .HasPrecision(18, 2);

                entity.Property(b => b.DiscountPrice)
                    .HasPrecision(18, 2);

                // Relationship with Admin
                entity.HasOne(b => b.Admin)
                    .WithMany()
                    .HasForeignKey(b => b.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Cart Configuration
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany(u => u.Carts)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Book)
                    .WithMany(b => b.Carts)
                    .HasForeignKey(c => c.BookId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(c => c.PricePerUnit)
                    .HasPrecision(18, 2);
            });
        }
    }
}