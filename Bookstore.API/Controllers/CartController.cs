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
    // DTO for adding to cart
    public class AddToCartRequest
    {
        public int Quantity { get; set; } = 1;
    }

    // DTO stub for purchase request
    public class PurchaseCartRequest
    {
        // Extend in future if needed
    }

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

        // Helper to get current user ID from JWT
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

        // Get user's cart
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();
            var cartItems = await _cartService.GetCartAsync(userId);

            var userEmail = User.FindFirstValue(ClaimTypes.Email);
            var totalQty = cartItems.Sum(c => c.Quantity);
            var totalPrice = cartItems.Sum(c => c.Quantity * c.PricePerUnit);

            var response = new
            {
                User = new { Id = userId, Email = userEmail },
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

        // Get only total quantity
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
            var cartItems = await _cartService.GetFullCartAsync(userId);
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

        // Add item to cart with quantity from body
        [HttpPost("{bookId}")]
        public async Task<IActionResult> AddToCart(
            int bookId,
            [FromBody] AddToCartRequest request
        )
        {
            try
            {
                await _cartService.AddToCartAsync(GetUserId(), bookId);
                return Ok(new
                {
                    Message = "Item added to cart",
                    BookId = bookId,
                    QuantityRequested = request.Quantity,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (InvalidOperationException ex)
                when (ex.Message.Contains("already purchased"))
            {
                return Conflict(new
                {
                    Status = "Error",
                    ErrorCode = "CART-ALREADY-PURCHASED",
                    Message = "This book is already purchased."
                });
            }
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

        // Remove item
        [HttpDelete("{cartId}")]
        public async Task<IActionResult> RemoveFromCart(int cartId)
        {
            await _cartService.RemoveFromCartAsync(cartId);
            return NoContent();
        }

        // Checkout with body parameter stub
        [HttpPost("purchase")]
        public async Task<IActionResult> PurchaseCart(
            [FromBody] PurchaseCartRequest req   // <-- added this
        )
        {
            var userId = GetUserId();
            var cartItems = await _cartService.GetCartAsync(userId);

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
