using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Bookstore.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Bookstore.Data.Interfaces;

namespace Bookstore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "USER")]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistService _wishlistService;
        private readonly IUserRepository _userRepo;

        public WishlistController(IWishlistService wishlistService, IUserRepository userRepo)
        {
            _wishlistService = wishlistService;
            _userRepo = userRepo;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        [HttpPost("{bookId}")]
        public async Task<IActionResult> AddToWishlist(int bookId)
        {
            try
            {
                var userId = GetUserId();
                var wishlistItem = await _wishlistService.AddToWishlistAsync(userId, bookId);
                var user = await _userRepo.GetUserByIdAsync(userId);

                return Ok(new WishlistResponseDto
                {
                    WishlistId = wishlistItem.Id,
                    BookId = wishlistItem.BookId,
                    AddedAt = wishlistItem.AddedAt,
                    User = new UserInfoDto
                    {
                        UserId = userId,
                        Email = user.Email,
                        FullName = $"{user.FirstName} {user.LastName}"
                    },
                    BookName = wishlistItem.Book.BookName,
                    Author = wishlistItem.Book.Author,
                    Price = wishlistItem.Book.Price
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete("{bookId}")]
        public async Task<IActionResult> RemoveFromWishlist(int bookId)
        {
            var userId = GetUserId();
            await _wishlistService.RemoveFromWishlistAsync(userId, bookId);
            return NoContent();
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = GetUserId();
            var wishlistItems = await _wishlistService.GetWishlistAsync(userId);

            var response = wishlistItems.Select(w => new WishlistResponseDto
            {
                WishlistId = w.Id,
                BookId = w.BookId,
                BookName = w.Book.BookName,
                Author = w.Book.Author,
                Price = w.Book.Price,
                AddedAt = w.AddedAt
            });

            return Ok(response);
        }
    }
}