using Bookstore.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Data.Interfaces
{
    public interface IAdminRepository
    {
        // Registration and Authentication
        Task<Admin> RegisterAdmin(Admin admin);
        Task<Admin?> LoginAdmin(string email, string password);

        // Existence Checks
        Task<bool> AdminExists(string email);

        // Get Operations
        Task<Admin?> GetByEmail(string email);
        Task<Admin?> GetByExternalId(string externalId);
        Task<IEnumerable<Admin>> GetAllAdminsAsync();
        Task<Admin?> GetAdminByIdAsync(int id);

        // Update/Delete Operations
        Task UpdateAdminAsync(Admin admin);
        Task DeleteAdminAsync(int id);
    }
}