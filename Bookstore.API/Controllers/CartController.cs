using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Business.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookstore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User")]
    public class CartController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        // Helper method to retrieve the current logged-in user's ID from the JWT token
        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // Retrieve the cart of the current user
        [HttpGet]
        public async Task<IActionResult> GetCart()
            => Ok(await _cartService.GetCartAsync(GetUserId()));

        // Add a book to the cart using its bookId
        [HttpPost("{bookId}")]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            await _cartService.AddToCartAsync(GetUserId(), bookId);
            return Ok();
        }

        // Update the quantity of a specific cart item
        [HttpPut("{cartId}")]
        public async Task<IActionResult> UpdateQuantity(int cartId, [FromBody] int quantity)
        {
            await _cartService.UpdateQuantityAsync(cartId, quantity);
            return Ok();
        }

        // Remove a book from the cart using its cartId
        [HttpDelete("{cartId}")]
        public async Task<IActionResult> RemoveFromCart(int cartId)
        {
            await _cartService.RemoveFromCartAsync(cartId);
            return NoContent();
        }

        // Purchase all items currently in the cart
        [HttpPost("purchase")]
        public async Task<IActionResult> PurchaseCart()
        {
            await _cartService.PurchaseCartAsync(GetUserId());
            return Ok(new { Message = "Purchase successful" });
        }
    }
}
