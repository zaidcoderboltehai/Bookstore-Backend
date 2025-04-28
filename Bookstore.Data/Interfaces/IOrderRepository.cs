using Bookstore.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Data.Interfaces
{
    /// <summary>
    /// Interface for order management operations
    /// </summary>
    public interface IOrderRepository
    {
        /// <summary>
        /// Create a new order in the system
        /// </summary>
        /// <param name="order">Order entity to create</param>
        /// <returns>Created order with generated ID</returns>
        Task<OrderSummary> CreateOrderAsync(OrderSummary order);

        /// <summary>
        /// Retrieve an order by its unique identifier
        /// </summary>
        /// <param name="id">Order ID to search for</param>
        /// <returns>Complete order details or null if not found</returns>
        Task<OrderSummary> GetOrderByIdAsync(int id);

        /// <summary>
        /// Get all orders for a specific user
        /// </summary>
        /// <param name="userId">User ID to retrieve orders for</param>
        /// <returns>Collection of user's order history</returns>
        Task<IEnumerable<OrderSummary>> GetUserOrdersAsync(int userId);
    }
}