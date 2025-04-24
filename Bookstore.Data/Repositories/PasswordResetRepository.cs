using System; // Basic .NET functionalities ko use karne ke liye
using System.Threading.Tasks; // Asynchronous programming ko support karne ke liye
using Bookstore.Data.Entities; // PasswordReset entity ko use karne ke liye
using Bookstore.Data.Interfaces; // Interface ko use karne ke liye
using Microsoft.EntityFrameworkCore; // Entity Framework Core ka use for DB operations

namespace Bookstore.Data.Repositories
{
    // PasswordResetRepository class jo IPasswordResetRepository interface ko implement kar rahi hai
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly AppDbContext _ctx; // AppDbContext ka reference, jo DB se interact karta hai

        // Constructor jo DbContext ko initialize karta hai
        public PasswordResetRepository(AppDbContext ctx) => _ctx = ctx;

        // Password reset request create karne ka method
        public async Task CreateAsync(PasswordReset reset)
        {
            _ctx.PasswordResets.Add(reset); // Password reset entity ko add karte hain DB mein
            await _ctx.SaveChangesAsync(); // Changes ko save karte hain DB mein
        }

        // Token ke basis par password reset request ko fetch karna
        public Task<PasswordReset?> GetByTokenAsync(Guid token) =>
            _ctx.PasswordResets.FirstOrDefaultAsync(r => r.Token == token); // DB se token ke basis par search karte hain

        // Password reset request ko delete karne ka method
        public async Task DeleteAsync(PasswordReset reset)
        {
            _ctx.PasswordResets.Remove(reset); // Password reset entity ko remove karte hain DB se
            await _ctx.SaveChangesAsync(); // Changes ko DB mein save karte hain
        }
    }
}
