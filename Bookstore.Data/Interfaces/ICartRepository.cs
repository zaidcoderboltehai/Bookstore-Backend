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
    }
}