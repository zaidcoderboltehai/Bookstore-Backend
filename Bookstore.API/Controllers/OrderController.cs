using Bookstore.Business.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Bookstore.API.Models;

namespace Bookstore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "USER")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _service;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderService service,
            ILogger<OrderController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                _logger.LogInformation("Creating new order for user {UserId}", userId);

                var order = await _service.CreateOrderFromCartAsync(userId, dto.AddressId);

                _logger.LogInformation("Order {OrderId} created successfully for user {UserId}",
                    order.Id, userId);

                return CreatedAtAction(nameof(GetOrder), new { orderId = order.Id }, order);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Order creation failed: {Message}", ex.Message);
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order for user {UserId}", GetAuthenticatedUserId());
                return StatusCode(500, new { Error = "An error occurred while creating the order" });
            }
        }

        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrder(int orderId)
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                _logger.LogDebug("Fetching order {OrderId} for user {UserId}", orderId, userId);

                var order = await _service.GetOrderByIdAsync(userId, orderId);

                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found for user {UserId}", orderId, userId);
                    return NotFound();
                }

                return Ok(order);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching order {OrderId}", orderId);
                return StatusCode(500, new { Error = "An error occurred while retrieving the order" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                _logger.LogDebug("Fetching order history for user {UserId}", userId);

                var orders = await _service.GetUserOrdersAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching orders for user {UserId}", GetAuthenticatedUserId());
                return StatusCode(500, new { Error = "An error occurred while retrieving orders" });
            }
        }

        private int GetAuthenticatedUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogError("Invalid user ID claim: {UserId}", userIdClaim);
                throw new UnauthorizedAccessException("Invalid user credentials");
            }
            return userId;
        }
    }
}