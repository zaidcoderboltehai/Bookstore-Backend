using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;

        public OrderRepository(AppDbContext context) => _context = context;

        public async Task<OrderSummary> CreateOrderAsync(OrderSummary order)
        {
            _context.OrderSummaries.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<OrderSummary> GetOrderByIdAsync(int id)
            => await _context.OrderSummaries
                .Include(os => os.OrderItems)
                .ThenInclude(oi => oi.Book)
                .FirstOrDefaultAsync(os => os.Id == id);

        public async Task<IEnumerable<OrderSummary>> GetUserOrdersAsync(int userId)
            => await _context.OrderSummaries
                .Where(os => os.UserId == userId)
                .Include(os => os.OrderItems)
                .ThenInclude(oi => oi.Book)
                .ToListAsync();
    }
}