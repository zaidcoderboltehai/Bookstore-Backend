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
        private readonly IUserRepository _userRepo;

        public UserAuthService(IUserRepository userRepo) => _userRepo = userRepo;

        public async Task<User> Register(User user, string password)
        {
            // Check if user already exists
            if (await _userRepo.UserExists(user.Email))
                throw new Exception("User already exists");

            // ✅ BCrypt EnhancedHashPassword with work factor 12 (Admin ke saath match)
            user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor: 12);

            // Database mein user create karo
            return await _userRepo.RegisterUser(user);
        }

        public async Task<User> Login(string email, string password)
        {
            // User ko email se fetch karo aur password verify karo
            var user = await _userRepo.LoginUser(email, password);

            // Agar user nahi mila ya password galat toh error
            return user ?? throw new Exception("Invalid credentials");
        }
    }
}