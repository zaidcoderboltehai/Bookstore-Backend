using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Business.Services
{
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepo;
        private readonly IBookRepository _bookRepo;

        public CartService(ICartRepository cartRepo, IBookRepository bookRepo)
        {
            _cartRepo = cartRepo;
            _bookRepo = bookRepo;
        }

        public async Task AddToCartAsync(int userId, int bookId)
        {
            // 1) Agar user already purchased this book
            var anyPurchased = await _cartRepo.ExistsPurchasedAsync(userId, bookId);
            if (anyPurchased)
                throw new InvalidOperationException("This book is already purchased.");

            // 2) Existing non-purchased item handle
            var existing = await _cartRepo.GetByUserAndBookAsync(userId, bookId);
            if (existing != null)
            {
                existing.Quantity++;
                await _cartRepo.UpdateAsync(existing);
            }
            else
            {
                var book = await _bookRepo.GetByIdAsync(bookId)
                    ?? throw new ArgumentException("Book not found");
                await _cartRepo.AddAsync(new Cart
                {
                    UserId = userId,
                    BookId = bookId,
                    PricePerUnit = book.Price,
                    Quantity = 1,
                    AddedAt = DateTime.UtcNow
                });
            }
        }

        public async Task UpdateQuantityAsync(int cartId, int quantity)
        {
            var cartItem = await _cartRepo.GetByIdAsync(cartId)
                ?? throw new ArgumentException("Cart item not found");

            cartItem.Quantity = Math.Max(1, quantity); // Quantity can never be less than 1
            await _cartRepo.UpdateAsync(cartItem);
        }

        public async Task RemoveFromCartAsync(int cartId)
        {
            var cartItem = await _cartRepo.GetByIdAsync(cartId);
            if (cartItem != null)
                await _cartRepo.DeleteAsync(cartItem);
        }

        public async Task<IEnumerable<Cart>> GetCartAsync(int userId)
            => await _cartRepo.GetUserCartAsync(userId);

        public async Task<IEnumerable<Cart>> GetFullCartAsync(int userId)
            => await _cartRepo.GetAllCartItemsAsync(userId);

        public async Task<IEnumerable<Cart>> PurchaseCartAsync(int userId)
        {
            var cartItems = (await _cartRepo.GetUserCartAsync(userId)).ToList();

            if (!cartItems.Any())
                throw new InvalidOperationException("Cart is empty");

            foreach (var item in cartItems)
            {
                item.IsPurchased = true;
                item.PurchasedAt = DateTime.UtcNow; // Time of purchase
                await _cartRepo.UpdateAsync(item);
            }

            return cartItems.Where(c => c.IsPurchased);
        }
    }
}
