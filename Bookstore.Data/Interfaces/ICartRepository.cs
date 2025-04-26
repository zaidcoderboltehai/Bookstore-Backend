using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Data.Entities;

namespace Bookstore.Data.Interfaces
{
    public interface ICartRepository
    {
        Task<Cart> AddAsync(Cart cart);
        Task UpdateAsync(Cart cart);
        Task DeleteAsync(Cart cart);
        Task<Cart> GetByIdAsync(int id);
        Task<IEnumerable<Cart>> GetUserCartAsync(int userId);
        Task<Cart> GetByUserAndBookAsync(int userId, int bookId);

        // Naya method: check if a user already purchased a specific book
        Task<bool> ExistsPurchasedAsync(int userId, int bookId);

        // Naya method: get all cart items including purchased ones
        Task<IEnumerable<Cart>> GetAllCartItemsAsync(int userId);
    }
}