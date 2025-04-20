// Web API banane ke liye necessary namespaces
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

// Admin se related entities aur database access ke liye
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using System.Threading.Tasks;

namespace Bookstore.API.Controllers
{
    // Ye batata hai ki ye ek API controller hai
    [ApiController]

    // Ye controller ka route hai (URL path): /AdminManagement
    [Route("[controller]")]

    // Sirf Admin role wale log hi is controller ko access kar sakte hain
    [Authorize(Roles = "Admin")]
    public class AdminManagementController : ControllerBase
    {
        // Admin data ke liye repository ka object
        private readonly IAdminRepository _repo;

        // Constructor jahan repository inject hoti hai (dependency injection)
        public AdminManagementController(IAdminRepository repo)
        {
            _repo = repo;
        }

        // GET method: Saare admins ko list karega
        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _repo.GetAllAdminsAsync());

        // GET method: Specific admin ko ID ke through laane ke liye
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            // ID ke basis pe admin laa rahe hain
            var admin = await _repo.GetAdminByIdAsync(id);

            // Agar admin nahi mila toh 404 NotFound return karega
            if (admin == null) return NotFound();

            // Agar mila toh 200 OK ke saath admin data return karega
            return Ok(admin);
        }

        // POST method: Naya admin create karne ke liye
        [HttpPost]
        public async Task<IActionResult> Create(Admin admin)
        {
            // Admin register ho raha hai
            var created = await _repo.RegisterAdmin(admin);

            // 201 Created ke saath naye admin ka ID bhi bhej rahe hain
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        // PUT method: Admin details update karne ke liye
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Admin updated)
        {
            // Agar URL wali ID aur updated admin ki ID match nahi karti toh error
            if (id != updated.Id) return BadRequest();

            // Admin update kar rahe hain
            await _repo.UpdateAdminAsync(updated);

            // 204 NoContent ka matlab update successful, lekin kuch return nahi kar rahe
            return NoContent();
        }

        // DELETE method: Admin ko ID ke basis pe delete karne ke liye
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            // Admin delete ho raha hai
            await _repo.DeleteAdminAsync(id);

            // Delete ke baad bhi 204 NoContent return hota hai
            return NoContent();
        }
    }
}
