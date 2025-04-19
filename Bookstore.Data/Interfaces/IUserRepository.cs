using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Data.Entities;

namespace Bookstore.Data.Interfaces
{
    public interface IUserRepository
    {
        // User Registration
        Task<User> RegisterUser(User user);

        // Authentication
        Task<User?> LoginUser(string email, string password);
        Task<bool> UserExists(string email);

        // User Management
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<User?> GetUserByIdAsync(int id);
        Task<User?> GetUserByEmail(string email);

        // Update/Delete Operations
        Task UpdateUserAsync(User user); // Updated to Async pattern
        Task DeleteUserAsync(int id);     // New delete method
    }
}