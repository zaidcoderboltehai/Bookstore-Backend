using BCrypt.Net; // Password hashing ke liye
using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using System;
using System.Threading.Tasks;

namespace Bookstore.Business.Services
{
    public class UserAuthService : IUserAuthService
    {
        private readonly IUserRepository _userRepo; // User repository ka instance, jo database se interact karega

        // Constructor: Jab UserAuthService banayi jaati hai, toh userRepo ko inject kiya jaata hai
        public UserAuthService(IUserRepository userRepo) => _userRepo = userRepo;

        // Register method: User ko register karne ke liye
        public async Task<User> Register(User user, string password)
        {
            // Check karo agar user already exist karta hai
            if (await _userRepo.UserExists(user.Email))
                throw new Exception("User already exists"); // Agar user already hai toh exception throw karo

            // ✅ BCrypt EnhancedHashPassword ke saath password hash kar rahe hain, work factor 12 (Admin ke saath match)
            user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor: 12);

            // Database mein user ko create karo aur save karo
            return await _userRepo.RegisterUser(user); // User ko repository mein save kar rahe hain
        }

        // Login method: User ko login karne ke liye
        public async Task<User> Login(string email, string password)
        {
            // User ko email se fetch karo aur password verify karo
            var user = await _userRepo.LoginUser(email, password); // Login ke liye user fetch kar rahe hain

            // Agar user nahi mila ya password galat ho, toh exception throw karo
            return user ?? throw new Exception("Invalid credentials"); // Agar user ya credentials galat hain toh error throw karega
        }
    }
}
