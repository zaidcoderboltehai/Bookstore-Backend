using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookstore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "USER")]
    public class CustomerAddressController : ControllerBase
    {
        private readonly ICustomerAddressService _service;
        private readonly ILogger<CustomerAddressController> _logger;

        public CustomerAddressController(
            ICustomerAddressService service,
            ILogger<CustomerAddressController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost]
        public async Task<IActionResult> AddAddress([FromBody] CustomerAddress address)
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                _logger.LogInformation("Adding new address for user {UserId}", userId);

                var createdAddress = await _service.AddAddressAsync(userId, address);

                _logger.LogInformation("Address {AddressId} created for user {UserId}",
                    createdAddress.Id, userId);

                return CreatedAtAction(nameof(GetById), new { id = createdAddress.Id }, createdAddress);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding address for user {UserId}", GetAuthenticatedUserId());
                return StatusCode(500, "An error occurred while adding the address");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userId = GetAuthenticatedUserId();
                _logger.LogDebug("Fetching addresses for user {UserId}", userId);

                var addresses = await _service.GetUserAddressesAsync(userId);
                return Ok(addresses);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching addresses for user {UserId}", GetAuthenticatedUserId());
                return StatusCode(500, "An error occurred while retrieving addresses");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogDebug("Fetching address {AddressId}", id);

                var address = await _service.GetAddressByIdAsync(id);

                if (address == null)
                {
                    _logger.LogWarning("Address {AddressId} not found", id);
                    return NotFound();
                }

                if (address.UserId != GetAuthenticatedUserId())
                {
                    _logger.LogWarning("Unauthorized access attempt to address {AddressId}", id);
                    return Forbid();
                }

                return Ok(address);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching address {AddressId}", id);
                return StatusCode(500, "An error occurred while retrieving the address");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CustomerAddress address)
        {
            try
            {
                _logger.LogInformation("Updating address {AddressId}", id);

                if (id != address.Id)
                {
                    _logger.LogWarning("ID mismatch in address update: {RouteId} vs {BodyId}",
                        id, address.Id);
                    return BadRequest("ID in route does not match ID in body");
                }

                if (address.UserId != GetAuthenticatedUserId())
                {
                    _logger.LogWarning("Unauthorized update attempt for address {AddressId}", id);
                    return Forbid();
                }

                await _service.UpdateAddressAsync(address);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Address {AddressId} not found for update", id);
                return NotFound();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating address {AddressId}", id);
                return StatusCode(500, "An error occurred while updating the address");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Deleting address {AddressId}", id);

                var address = await _service.GetAddressByIdAsync(id);

                if (address == null)
                {
                    _logger.LogWarning("Address {AddressId} not found for deletion", id);
                    return NotFound();
                }

                if (address.UserId != GetAuthenticatedUserId())
                {
                    _logger.LogWarning("Unauthorized delete attempt for address {AddressId}", id);
                    return Forbid();
                }

                await _service.DeleteAddressAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting address {AddressId}", id);
                return StatusCode(500, "An error occurred while deleting the address");
            }
        }

        private int GetAuthenticatedUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userId, out int parsedId))
            {
                _logger.LogError("Invalid user ID claim: {UserId}", userId);
                throw new UnauthorizedAccessException("Invalid user credentials");
            }
            return parsedId;
        }
    }
}