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
        public DbSet<PasswordReset> PasswordResets { get; set; } = null!; // ✅ Added for forgot‑password flow
    }
}
