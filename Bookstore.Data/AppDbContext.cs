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

        // Database Tables
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<PasswordReset> PasswordResets { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<OrderSummary> OrderSummaries { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //----------- User Config ------------
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
            });

            //----------- Admin Config -----------
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasIndex(a => a.Email).IsUnique();
                entity.HasIndex(a => a.ExternalId).IsUnique();
            });

            //----------- Book Config ------------
            modelBuilder.Entity<Book>(entity =>
            {
                entity.Property(b => b.Price).HasPrecision(18, 2);
                entity.Property(b => b.DiscountPrice).HasPrecision(18, 2);

                entity.HasOne(b => b.Admin)
                    .WithMany()
                    .HasForeignKey(b => b.AdminId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //----------- Cart Config ------------
            modelBuilder.Entity<Cart>(entity =>
            {
                entity.HasOne(c => c.User)
                    .WithMany(u => u.Carts)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Book)
                    .WithMany(b => b.Carts)
                    .HasForeignKey(c => c.BookId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //----------- Wishlist Config --------
            modelBuilder.Entity<Wishlist>(entity =>
            {
                entity.HasIndex(w => new { w.UserId, w.BookId }).IsUnique();

                entity.HasOne(w => w.User)
                    .WithMany(u => u.Wishlists)
                    .HasForeignKey(w => w.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.Book)
                    .WithMany(b => b.Wishlists)
                    .HasForeignKey(w => w.BookId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //----------- Address Config --------
            modelBuilder.Entity<CustomerAddress>(entity =>
            {
                entity.HasOne(ca => ca.User)
                    .WithMany(u => u.CustomerAddresses)
                    .HasForeignKey(ca => ca.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            //----------- Order Config -----------
            modelBuilder.Entity<OrderSummary>(entity =>
            {
                // Fixed Cascade Conflict
                entity.HasOne(os => os.User)
                    .WithMany(u => u.Orders)
                    .HasForeignKey(os => os.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(os => os.CustomerAddress)
                    .WithMany()
                    .HasForeignKey(os => os.CustomerAddressId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.Property(os => os.TotalAmount)
                    .HasPrecision(18, 2);
            });

            //----------- Order Item Config ------
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(oi => oi.Book)
                    .WithMany(b => b.OrderItems)
                    .HasForeignKey(oi => oi.BookId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            //----------- Other Configs ----------
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(rt => rt.Token).IsUnique();
            });

            modelBuilder.Entity<PasswordReset>(entity =>
            {
                entity.HasKey(pr => pr.Token);
            });
        }
    }
}