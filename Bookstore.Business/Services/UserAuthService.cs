using BCrypt.Net;
using Bookstore.Business.Interfaces;
using System.Threading.Tasks;
using System;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;

namespace Bookstore.Business.Services
{
    public class UserAuthService : IUserAuthService
    {
        private readonly IUserRepository _userRepo;

        public UserAuthService(IUserRepository userRepo) => _userRepo = userRepo;

        public async Task<User> Register(User user, string password)
        {
            if (await _userRepo.UserExists(user.Email))
                throw new Exception("User already exists");

            user.Password = BCrypt.Net.BCrypt.HashPassword(password);
            return await _userRepo.RegisterUser(user);
        }

        // Corrected Return Type (User instead of string)
        public async Task<User> Login(string email, string password)
        {
            var user = await _userRepo.LoginUser(email, password);
            return user ?? throw new Exception("Invalid credentials");
        }
    }
}