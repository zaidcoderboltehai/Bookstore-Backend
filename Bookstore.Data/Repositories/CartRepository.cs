using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly AppDbContext _context;

        public CartRepository(AppDbContext context) => _context = context;

        public async Task<Cart> AddAsync(Cart cart)
        {
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
            return cart;
        }

        public async Task UpdateAsync(Cart cart)
        {
            _context.Entry(cart).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Cart cart)
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }

        public async Task<Cart> GetByIdAsync(int id)
            => await _context.Carts.FindAsync(id);

        public async Task<IEnumerable<Cart>> GetUserCartAsync(int userId)
            => await _context.Carts
                .Where(c => c.UserId == userId && !c.IsPurchased)
                .Include(c => c.Book)
                .ToListAsync();

        public async Task<Cart> GetByUserAndBookAsync(int userId, int bookId)
            => await _context.Carts
                .FirstOrDefaultAsync(c =>
                    c.UserId == userId &&
                    c.BookId == bookId &&
                    !c.IsPurchased);

        public Task<bool> ExistsPurchasedAsync(int userId, int bookId) =>
            _context.Carts.AnyAsync(c => c.UserId == userId
                                      && c.BookId == bookId
                                      && c.IsPurchased);

        public async Task<IEnumerable<Cart>> GetAllCartItemsAsync(int userId)
            => await _context.Carts
                .Where(c => c.UserId == userId)
                .Include(c => c.Book)
                .ToListAsync();
    }
}
