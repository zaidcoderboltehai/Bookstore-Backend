using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Data.Entities;

namespace Bookstore.Business.Interfaces
{
    public interface IWishlistService
    {
        Task<Wishlist> AddToWishlistAsync(int userId, int bookId);
        Task RemoveFromWishlistAsync(int userId, int bookId);
        Task<IEnumerable<Wishlist>> GetWishlistAsync(int userId);
    }
}