// Web API controller banane ke liye required namespaces
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

// Admin entity aur repository interface ko use karne ke liye
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using System.Threading.Tasks;

namespace Bookstore.API.Controllers
{
    // 👇 Ye batata hai ki ye ek API controller hai
    [ApiController]

    // 👇 URL route: api/AdminManagement
    [Route("[controller]")]

    // 👇 Sirf Admin role wale users hi ye controller use kar sakte hain
    [Authorize(Roles = "Admin")]
    public class AdminManagementController : ControllerBase
    {
        // 👇 Ye repository interface inject kiya gaya hai for database operations
        private readonly IAdminRepository _repo;

        // 👇 Constructor ke through repository mil rahi hai
        public AdminManagementController(IAdminRepository repo)
        {
            _repo = repo;
        }

        // 👇 GET: api/AdminManagement
        // 👇 Saare admins ki list return karega
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _repo.GetAllAdminsAsync());

        // 👇 GET: api/AdminManagement/5 (specific id se admin ko get karna)
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var admin = await _repo.GetAdminByIdAsync(id);

            // 👇 Agar admin nahi mila toh 404 error bhej dena
            if (admin == null) return NotFound();

            // 👇 Admin mila toh 200 OK ke saath data bhejna
            return Ok(admin);
        }

        // 👇 POST: api/AdminManagement
        // 👇 Naya admin create karne ke liye endpoint
        [HttpPost]
        public async Task<IActionResult> Create(Admin admin)
        {
            var created = await _repo.RegisterAdmin(admin);

            // 👇 Response mein 201 Created + location header with created admin ka ID
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        // 👇 PUT: api/AdminManagement/5
        // 👇 Kisi existing admin ko update karne ke liye
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Admin updated)
        {
            // 👇 ID match nahi hua toh bad request
            if (id != updated.Id) return BadRequest();

            // 👇 Update admin data
            await _repo.UpdateAdminAsync(updated);

            // 👇 204 No Content (successful update, koi content nahi return)
            return NoContent();
        }

        // 👇 DELETE: api/AdminManagement/5
        // 👇 Specific admin ko delete karne ke liye
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAdminAsync(id);

            // 👇 Delete ke baad bhi 204 No Content
            return NoContent();
        }
    }
}
