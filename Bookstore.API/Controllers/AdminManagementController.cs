// Required namespaces to build a Web API
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

// For admin-related entities and database access
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using System.Threading.Tasks;

namespace Bookstore.API.Controllers
{
    // Indicates that this is an API controller
    [ApiController]

    // Route for this controller (URL path): /AdminManagement
    [Route("[controller]")]

    // Only users with the Admin role can access this controller
    [Authorize(Roles = "ADMIN")]
    public class AdminManagementController : ControllerBase
    {
        // Repository object for accessing admin data
        private readonly IAdminRepository _repo;

        // Constructor where the repository is injected (dependency injection)
        public AdminManagementController(IAdminRepository repo)
        {
            _repo = repo;
        }

        // GET method: Will list all admins
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _repo.GetAllAdminsAsync());

        // GET method: To fetch a specific admin using ID
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // Fetching admin based on ID
            var admin = await _repo.GetAdminByIdAsync(id);

            // If admin not found, return 404 NotFound
            if (admin == null) return NotFound();

            // If found, return admin data with 200 OK
            return Ok(admin);
        }

        // POST method: To create a new admin
        [HttpPost]
        public async Task<IActionResult> Create(Admin admin)
        {
            // Registering a new admin
            var created = await _repo.RegisterAdmin(admin);

            // Returning 201 Created with the new admin's ID
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        // PUT method: To update admin details
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Admin updated)
        {
            // If the ID from URL and the updated admin's ID don't match, return error
            if (id != updated.Id) return BadRequest();

            // Updating the admin
            await _repo.UpdateAdminAsync(updated);

            // 204 NoContent means update was successful but nothing is returned
            return NoContent();
        }

        // DELETE method: To delete an admin by ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Deleting the admin
            await _repo.DeleteAdminAsync(id);

            // After deletion, 204 NoContent is returned
            return NoContent();
        }
    }
}
