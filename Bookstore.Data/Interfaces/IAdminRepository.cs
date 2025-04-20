using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Data.Entities;

namespace Bookstore.Data.Interfaces
{
    public interface IAdminRepository
    {
        // Register ek naya admin
        Task<Admin> RegisterAdmin(Admin admin);

        // Email/password ke saath login karna (nullable return type, agar admin nahi mila toh null return hoga)
        Task<Admin?> LoginAdmin(string email, string password);

        // Check karna ki admin email ke saath exist karta hai ya nahi
        Task<bool> AdminExists(string email);

        // Admin ko email ke basis pe fetch karna (authentication verification ke liye)
        Task<Admin> GetByEmail(string email);

        // 👇 Naye CRUD Methods
        // Sabhi admins ko get karna
        Task<IEnumerable<Admin>> GetAllAdminsAsync();

        // Admin ko ID ke basis pe get karna
        Task<Admin?> GetAdminByIdAsync(int id);

        // Admin ki details update karna
        Task UpdateAdminAsync(Admin admin);

        // Admin ko delete karna ID ke basis pe
        Task DeleteAdminAsync(int id);
    }
}
