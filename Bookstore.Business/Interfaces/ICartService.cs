using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Data.Entities;

namespace Bookstore.Business.Interfaces
{
    public interface ICartService
    {
        Task AddToCartAsync(int userId, int bookId);
        Task UpdateQuantityAsync(int cartId, int quantity);
        Task RemoveFromCartAsync(int cartId);
        Task<IEnumerable<Cart>> GetCartAsync(int userId);
        Task PurchaseCartAsync(int userId);
    }
}