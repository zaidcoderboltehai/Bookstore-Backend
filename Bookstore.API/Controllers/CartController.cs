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

        private int GetUserId() =>
            int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        [HttpGet]
        public async Task<IActionResult> GetCart()
            => Ok(await _cartService.GetCartAsync(GetUserId()));

        [HttpPost("{bookId}")]
        public async Task<IActionResult> AddToCart(int bookId)
        {
            await _cartService.AddToCartAsync(GetUserId(), bookId);
            return Ok();
        }

        [HttpPut("{cartId}")]
        public async Task<IActionResult> UpdateQuantity(int cartId, [FromBody] int quantity)
        {
            await _cartService.UpdateQuantityAsync(cartId, quantity);
            return Ok();
        }

        [HttpDelete("{cartId}")]
        public async Task<IActionResult> RemoveFromCart(int cartId)
        {
            await _cartService.RemoveFromCartAsync(cartId);
            return NoContent();
        }

        [HttpPost("purchase")]
        public async Task<IActionResult> PurchaseCart()
        {
            await _cartService.PurchaseCartAsync(GetUserId());
            return Ok(new { Message = "Purchase successful" });
        }
    }
}