using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using System;
using System.Collections.Generic;
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
            var book = await _bookRepo.GetByIdAsync(bookId)
                ?? throw new ArgumentException("Book not found");

            var existingItem = await _cartRepo.GetByUserAndBookAsync(userId, bookId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
                await _cartRepo.UpdateAsync(existingItem);
            }
            else
            {
                await _cartRepo.AddAsync(new Cart
                {
                    UserId = userId,
                    BookId = bookId,
                    PricePerUnit = book.Price,
                    Quantity = 1
                });
            }
        }

        public async Task UpdateQuantityAsync(int cartId, int quantity)
        {
            var cartItem = await _cartRepo.GetByIdAsync(cartId)
                ?? throw new ArgumentException("Cart item not found");

            cartItem.Quantity = quantity > 0 ? quantity : 1;
            await _cartRepo.UpdateAsync(cartItem);
        }

        public async Task RemoveFromCartAsync(int cartId)
        {
            var cartItem = await _cartRepo.GetByIdAsync(cartId);
            if (cartItem != null) await _cartRepo.DeleteAsync(cartItem);
        }

        public async Task<IEnumerable<Cart>> GetCartAsync(int userId)
            => await _cartRepo.GetUserCartAsync(userId);

        public async Task PurchaseCartAsync(int userId)
        {
            var cartItems = await _cartRepo.GetUserCartAsync(userId);
            foreach (var item in cartItems)
            {
                item.IsPurchased = true;
                await _cartRepo.UpdateAsync(item);
            }
        }
    }
}
