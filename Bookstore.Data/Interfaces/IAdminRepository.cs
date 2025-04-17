using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Data.Entities;

namespace Bookstore.Data.Interfaces
{
    public interface IAdminRepository
    {
        // Register a new admin
        Task<Admin> RegisterAdmin(Admin admin);

        // Login with email/password (nullable return)
        Task<Admin?> LoginAdmin(string email, string password);

        // Check if admin exists by email
        Task<bool> AdminExists(string email);

        // Get admin by email (for auth verification)
        Task<Admin> GetByEmail(string email);

        // 👇 New CRUD Methods
        Task<IEnumerable<Admin>> GetAllAdminsAsync();
        Task<Admin?> GetAdminByIdAsync(int id);
        Task UpdateAdminAsync(Admin admin);
        Task DeleteAdminAsync(int id);
    }
}
