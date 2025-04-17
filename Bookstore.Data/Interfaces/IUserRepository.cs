using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Data.Entities;

namespace Bookstore.Data.Interfaces
{
    public interface IUserRepository
    {
        Task<User> RegisterUser(User user);
        Task<User?> LoginUser(string email, string password);
        Task<bool> UserExists(string email);

        // Existing methods
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);

        // New methods
        Task<User?> GetUserByEmail(string email);
        Task UpdateUser(User user); // ✅ Added for password reset flow
    }
}