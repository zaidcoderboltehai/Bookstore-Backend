using Bookstore.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Business.Interfaces
{
    public interface IOrderService
    {
        // ✅ Updated method signature with address ID
        Task<OrderSummary> CreateOrderFromCartAsync(int userId, int addressId);
        Task<OrderSummary> GetOrderByIdAsync(int userId, int orderId);
        Task<IEnumerable<OrderSummary>> GetUserOrdersAsync(int userId);
    }
}