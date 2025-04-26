using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Business.Services
{
    public class WishlistService : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepo;
        private readonly IBookRepository _bookRepo;

        public WishlistService(IWishlistRepository wishlistRepo, IBookRepository bookRepo)
        {
            _wishlistRepo = wishlistRepo;
            _bookRepo = bookRepo;
        }

        public async Task<Wishlist> AddToWishlistAsync(int userId, int bookId)
        {
            var book = await _bookRepo.GetByIdAsync(bookId);
            if (book == null)
                throw new ArgumentException("Book not found");

            if (await _wishlistRepo.BookExistsInWishlist(userId, bookId))
                throw new InvalidOperationException("Book already in wishlist");

            var wishlistItem = new Wishlist
            {
                UserId = userId,
                BookId = bookId
            };

            return await _wishlistRepo.AddAsync(wishlistItem);
        }

        public async Task RemoveFromWishlistAsync(int userId, int bookId)
        {
            var wishlistItem = await _wishlistRepo.GetByUserAndBookAsync(userId, bookId);
            if (wishlistItem != null)
                await _wishlistRepo.DeleteAsync(wishlistItem);
        }

        public async Task<IEnumerable<Wishlist>> GetWishlistAsync(int userId)
            => await _wishlistRepo.GetByUserIdAsync(userId);
    }
}