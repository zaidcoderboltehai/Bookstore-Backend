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

        // Main Entities
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; } = null!;

        // Security Related Entities
        public DbSet<PasswordReset> PasswordResets { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!; // ✅ Added for token refresh functionality

        // Optional: Add model configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure RefreshToken entity
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(rt => rt.Token).IsUnique();
                entity.Property(rt => rt.Expires).IsRequired();
            });
        }
    }
}