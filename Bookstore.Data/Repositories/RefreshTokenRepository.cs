using Bookstore.Data.Entities; // RefreshToken entity ko use karne ke liye
using Bookstore.Data.Interfaces; // Interface ko use karne ke liye
using Microsoft.EntityFrameworkCore; // Entity Framework Core ka use for DB operations
using System.Collections.Generic; // Collection types ko handle karne ke liye
using System.Linq; // LINQ queries ke liye
using System.Threading.Tasks; // Asynchronous programming ko support karne ke liye

namespace Bookstore.Data.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context; // DbContext ka reference, jo DB se interact karta hai

        // Constructor jo context ko initialize karta hai
        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        // Refresh token create karne ka method
        public async Task<RefreshToken> CreateAsync(RefreshToken token)
        {
            _context.RefreshTokens.Add(token); // Token ko DB mein add karte hain
            await _context.SaveChangesAsync(); // Changes ko DB mein save karte hain
            return token; // Created token ko return karte hain
        }

        // Token ke basis par refresh token find karne ka method
        public async Task<RefreshToken?> FindByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token); // DB se token ke basis par search karte hain
        }

        // Refresh token ko delete karne ka method
        public async Task DeleteAsync(int id)
        {
            var token = await _context.RefreshTokens.FindAsync(id); // Token ko id ke basis par search karte hain
            if (token != null) // Agar token milta hai
            {
                _context.RefreshTokens.Remove(token); // Token ko DB se remove karte hain
                await _context.SaveChangesAsync(); // Changes ko DB mein save karte hain
            }
        }

        // ✅ Added missing interface implementation (User-specific tokens fetch karne ke liye)
        public async Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId) // UserId ke basis par refresh tokens fetch karte hain
                .ToListAsync(); // Results ko list mein convert karte hain
        }
    }
}
