using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Business.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Bookstore.API.Models;
using System;

namespace Bookstore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "USER")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // Helper method to get current user ID from JWT
        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Debug endpoint to check authentication and roles
        [HttpGet("debug-auth")]
        [AllowAnonymous]
        public IActionResult DebugAuth()
        {
            var isAuthenticated = User.Identity.IsAuthenticated;
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            var roles = User.Claims.Where(c => c.Type == "role" || c.Type == ClaimTypes.Role)
                                  .Select(c => c.Value).ToList();

            return Ok(new
            {
                IsAuthenticated = isAuthenticated,
                Claims = claims,
                Roles = roles,
                HasUserRole = User.IsInRole("USER")
            });
        }

        // Get user's cart with book details, user info, total quantity, total price, and purchase flags
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();
            var cartItems = await _cartService.GetCartAsync(userId);

            // User details from JWT
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // calculate total quantity and total price
            var totalQty = cartItems.Sum(c => c.Quantity);
            var totalPrice = cartItems.Sum(c => c.Quantity * c.PricePerUnit);

            var response = new
            {
                User = new
                {
                    Id = userId,
                    Email = userEmail
                },
                TotalQuantity = totalQty,
                TotalPrice = totalPrice,
                Items = cartItems.Select(c => new CartResponseDto
                {
                    Id = c.Id,
                    BookId = c.BookId,
                    BookName = c.Book!.BookName,
                    Author = c.Book.Author,
                    PricePerUnit = c.PricePerUnit,
                    Quantity = c.Quantity,
                    AddedAt = c.AddedAt,
                    IsPurchased = c.IsPurchased,
                    PurchasedAt = c.PurchasedAt
                })
            };

            return Ok(response);
        }

        // Get only total quantity of items in cart
        [HttpGet("total-quantity")]
        public async Task<IActionResult> GetTotalQuantity()
        {
            var qty = (await _cartService.GetCartAsync(GetUserId()))
                        .Sum(c => c.Quantity);
            return Ok(new { TotalQuantity = qty });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetFullCart()
        {
            var userId = GetUserId();
            var cartItems = await _cartService.GetFullCartAsync(userId); // includes both purchased & non-purchased

            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            var response = new
            {
                User = new { Id = userId, Email = userEmail },
                Items = cartItems.Select(c => new CartResponseDto
                {
                    Id = c.Id,
                    BookId = c.BookId,
                    BookName = c.Book.BookName,
                    Author = c.Book.Author,
                    PricePerUnit = c.PricePerUnit,
                    Quantity = c.Quantity,
                    AddedAt = c.AddedAt,
                    IsPurchased = c.IsPurchased,
                    PurchasedAt = c.PurchasedAt
                })
            };

            return Ok(response);
        }

        // Add item to cart
        [HttpPost("{bookId}")]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            await _cartService.AddToCartAsync(GetUserId(), bookId);
            return Ok(new
            {
                Message = "Item added to cart",
                BookId = bookId,
                Timestamp = DateTime.UtcNow
            });
        }

        // Update item quantity
        [HttpPut("{cartId}")]
        public async Task<IActionResult> UpdateQuantity(
            int cartId,
            [FromBody] int quantity)
        {
            if (quantity < 1)
                return BadRequest(new
                {
                    ErrorCode = "CART-100",
                    Message = "Quantity must be at least 1"
                });

            await _cartService.UpdateQuantityAsync(cartId, quantity);
            return Ok(new
            {
                CartId = cartId,
                NewQuantity = quantity,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Remove item from cart
        [HttpDelete("{cartId}")]
        public async Task<IActionResult> RemoveFromCart(int cartId)
        {
            await _cartService.RemoveFromCartAsync(cartId);
            return NoContent();
        }

        // Checkout cart with duplicate-check, then detailed response
        [HttpPost("purchase")]
        public async Task<IActionResult> PurchaseCart()
        {
            var userId = GetUserId();
            var cartItems = await _cartService.GetCartAsync(userId);

            // duplicate BookId check
            var dupGroup = cartItems
                           .Where(c => !c.IsPurchased)
                           .GroupBy(c => c.BookId)
                           .FirstOrDefault(g => g.Count() > 1);
            if (dupGroup != null)
                return BadRequest(new
                {
                    ErrorCode = "CART-200",
                    Message = $"BookId {dupGroup.Key} multiple times in cart. Remove duplicates before purchase."
                });

            var purchasedItems = await _cartService.PurchaseCartAsync(userId);

            return Ok(new
            {
                Message = "Purchase completed successfully",
                Summary = new
                {
                    TotalItems = purchasedItems.Count(),
                    GrandTotal = purchasedItems.Sum(i => i.PricePerUnit * i.Quantity)
                },
                Details = purchasedItems.Select(i => new {
                    i.Id,
                    i.BookId,
                    i.Quantity,
                    UnitPrice = i.PricePerUnit,
                    TotalPrice = i.PricePerUnit * i.Quantity,
                    PurchasedAt = i.PurchasedAt
                }),
                ReceiptDate = DateTime.UtcNow
            });
        }
    }
}
