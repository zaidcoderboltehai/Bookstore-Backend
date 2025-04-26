using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly AppDbContext _context;

        public WishlistRepository(AppDbContext context) => _context = context;

        // Add new wishlist item
        public async Task<Wishlist> AddAsync(Wishlist wishlist)
        {
            _context.Wishlists.Add(wishlist);
            await _context.SaveChangesAsync();
            return wishlist;
        }

        // Remove wishlist item
        public async Task DeleteAsync(Wishlist wishlist)
        {
            _context.Wishlists.Remove(wishlist);
            await _context.SaveChangesAsync();
        }

        // Get all wishlist items for a user with book details
        public async Task<IEnumerable<Wishlist>> GetByUserIdAsync(int userId)
            => await _context.Wishlists
                .Where(w => w.UserId == userId)
                .Include(w => w.Book) // ✅ Eager loading book details
                .ToListAsync();

        // Find specific wishlist item
        public async Task<Wishlist> GetByUserAndBookAsync(int userId, int bookId)
            => await _context.Wishlists
                .FirstOrDefaultAsync(w => w.UserId == userId && w.BookId == bookId);

        // Check if book exists in user's wishlist
        public async Task<bool> BookExistsInWishlist(int userId, int bookId)
            => await _context.Wishlists
                .AnyAsync(w => w.UserId == userId && w.BookId == bookId);
    }
}