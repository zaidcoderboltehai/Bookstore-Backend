using Microsoft.EntityFrameworkCore; // Entity Framework Core ka use DB se interact karne ke liye
using Bookstore.Data.Entities; // Entities ko use karne ke liye

namespace Bookstore.Data
{
    public class AppDbContext : DbContext
    {
        // Constructor jo DbContextOptions ko pass karta hai base class ke constructor ko
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Main Entities, jo DB mein store hongi
        public DbSet<User> Users { get; set; } = null!; // Users table ko represent karta hai
        public DbSet<Admin> Admins { get; set; } = null!; // Admins table ko represent karta hai

        // Security Related Entities, jo password reset aur token refresh ke liye hain
        public DbSet<PasswordReset> PasswordResets { get; set; } = null!; // PasswordReset table ko represent karta hai
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!; // ✅ Token refresh functionality ke liye added entity

        // Optional: Add model configurations, jahan hum tables ka behavior define kar sakte hain
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // RefreshToken entity ko configure karte hain
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(rt => rt.Token).IsUnique(); // Token ko unique banate hain DB mein
                entity.Property(rt => rt.Expires).IsRequired(); // Expires property ko required bana dete hain
            });
        }
    }
}
