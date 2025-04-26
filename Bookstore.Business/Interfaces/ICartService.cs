using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Data.Entities;

namespace Bookstore.Business.Interfaces
{
    public interface ICartService
    {
        /// <summary>
        /// Add a new item to the cart
        /// </summary>
        Task AddToCartAsync(int userId, int bookId);

        /// <summary>
        /// Update the quantity of a cart item
        /// </summary>
        Task UpdateQuantityAsync(int cartId, int quantity);

        /// <summary>
        /// Remove an item from the cart
        /// </summary>
        Task RemoveFromCartAsync(int cartId);

        /// <summary>
        /// Get the full cart details for a user
        /// </summary>
        Task<IEnumerable<Cart>> GetCartAsync(int userId);

        /// <summary>
        /// Get all cart items (including purchased and non-purchased)
        /// </summary>
        Task<IEnumerable<Cart>> GetFullCartAsync(int userId);

        /// <summary>
        /// Complete the purchase of the cart and return purchased items
        /// </summary>
        Task<IEnumerable<Cart>> PurchaseCartAsync(int userId);
    }
}
