using System.Threading.Tasks; // Asynchronous task ke liye necessary library
using Bookstore.Data.Entities; // User entity ko import kar rahe hain

namespace Bookstore.Business.Interfaces
{
    // IUserAuthService interface banayi gayi hai jo user authentication related operations ko define karegi
    public interface IUserAuthService
    {
        // Register method jo user ko register karega, yeh ek asynchronous task hai
        // Input: User object aur password, Output: Task<User> - jo user object ko return karega after registration
        Task<User> Register(User user, string password);

        // Login method jo user ko login karega
        // Input: Email aur password, Output: Task<User> - jo user object ko return karega agar login successful ho
        Task<User> Login(string email, string password); // Fixed return type - Pehle yeh string tha, ab User type return hoga
    }
}
