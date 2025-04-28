using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Business.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly ICartRepository _cartRepo;
        private readonly IBookRepository _bookRepo;
        private readonly ICustomerAddressRepository _addressRepo;

        public OrderService(
            IOrderRepository orderRepo,
            ICartRepository cartRepo,
            IBookRepository bookRepo,
            ICustomerAddressRepository addressRepo)
        {
            _orderRepo = orderRepo;
            _cartRepo = cartRepo;
            _bookRepo = bookRepo;
            _addressRepo = addressRepo;
        }

        public async Task<OrderSummary> CreateOrderFromCartAsync(int userId, int addressId)
        {
            var address = await _addressRepo.GetByIdAsync(addressId);
            if (address?.UserId != userId)
                throw new InvalidOperationException("Invalid shipping address");

            var cartItems = (await _cartRepo.GetUserCartAsync(userId)).ToList();
            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty");

            var order = new OrderSummary
            {
                UserId = userId,
                CustomerAddressId = addressId,
                TotalAmount = cartItems.Sum(ci => ci.Quantity * ci.PricePerUnit),
                OrderItems = cartItems.Select(ci => new OrderItem
                {
                    BookId = ci.BookId,
                    Quantity = ci.Quantity,
                    PricePerUnit = ci.PricePerUnit
                }).ToList()
            };

            var createdOrder = await _orderRepo.CreateOrderAsync(order);

            foreach (var cartItem in cartItems)
            {
                cartItem.IsPurchased = true;
                cartItem.PurchasedAt = DateTime.UtcNow;
                await _cartRepo.UpdateAsync(cartItem);
            }

            return createdOrder;
        }

        public async Task<OrderSummary> GetOrderByIdAsync(int userId, int orderId)
        {
            var order = await _orderRepo.GetOrderByIdAsync(orderId);
            return order?.UserId == userId ? order : null;
        }

        public async Task<IEnumerable<OrderSummary>> GetUserOrdersAsync(int userId)
            => await _orderRepo.GetUserOrdersAsync(userId);
    }
}