using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using System.Threading.Tasks;

namespace Bookstore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminManagementController : ControllerBase
    {
        private readonly IAdminRepository _repo;

        public AdminManagementController(IAdminRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
            => Ok(await _repo.GetAllAdminsAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var admin = await _repo.GetAdminByIdAsync(id);
            if (admin == null) return NotFound();
            return Ok(admin);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Admin admin)
        {
            var created = await _repo.RegisterAdmin(admin);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Admin updated)
        {
            if (id != updated.Id) return BadRequest();
            await _repo.UpdateAdminAsync(updated);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _repo.DeleteAdminAsync(id);
            return NoContent();
        }
    }
}
