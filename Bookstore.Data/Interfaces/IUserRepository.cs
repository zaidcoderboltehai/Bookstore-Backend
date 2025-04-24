using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Data.Entities;

namespace Bookstore.Data.Interfaces
{
    public interface IUserRepository
    {
        // User registration ka function
        Task<User> RegisterUser(User user);

        // User ko email aur password ke saath authenticate karna
        Task<User?> LoginUser(string email, string password);

        // Check karna ki user already exists hai ya nahi (email ke basis pe)
        Task<bool> UserExists(string email);

        // Sabhi users ko fetch karne ka function
        Task<IEnumerable<User>> GetAllUsersAsync();

        // User ko unke ID ke basis pe fetch karne ka function
        Task<User?> GetUserByIdAsync(int id);

        // Email ke through user ko fetch karne ka function
        Task<User?> GetUserByEmail(string email);

        // User ki details ko update karna (async pattern ke saath)
        Task UpdateUserAsync(User user); // Updated to Async pattern

        // User ko delete karne ka function (ID ke basis pe)
        Task DeleteUserAsync(int id);     // New delete method
    }
}
