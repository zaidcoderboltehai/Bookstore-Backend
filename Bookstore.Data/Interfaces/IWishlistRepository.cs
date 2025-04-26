using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Data.Entities;

namespace Bookstore.Data.Interfaces
{
    public interface IWishlistRepository
    {
        Task<Wishlist> AddAsync(Wishlist wishlist);
        Task DeleteAsync(Wishlist wishlist);
        Task<IEnumerable<Wishlist>> GetByUserIdAsync(int userId);
        Task<Wishlist> GetByUserAndBookAsync(int userId, int bookId);
        Task<bool> BookExistsInWishlist(int userId, int bookId);
    }
}