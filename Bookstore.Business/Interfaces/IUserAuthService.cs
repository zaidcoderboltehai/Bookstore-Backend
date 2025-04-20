using System.Threading.Tasks; // Asynchronous task ke liye necessary library
using Bookstore.Data.Entities; // User entity ko import kar rahe hain

namespace Bookstore.Business.Interfaces
{
    // IUserAuthService interface banayi gayi hai jo user authentication related operations ko define karegi
    public interface IUserAuthService
    {
        // Register method jo user ko register karega, yeh ek asynchronous task hai
        // Input: User object aur password, Output: Task<User> - jo user object ko return karega after registration
        // Yeh method user ko register karega aur successfully register hone par user ka object return karega
        Task<User> Register(User user, string password);

        // Login method jo user ko login karega
        // Input: Email aur password, Output: Task<User> - jo user object ko return karega agar login successful ho
        // Login ke baad, agar email aur password match karte hain toh user ka object return hoga
        Task<User> Login(string email, string password); // Fixed return type - Pehle yeh string tha, ab User type return hoga
    }
}
