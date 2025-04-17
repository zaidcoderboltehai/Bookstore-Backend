using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace Bookstore.Business.Services
{
    public class AdminAuthService : IAdminAuthService
    {
        private readonly IAdminRepository _adminRepo;
        private readonly string _validSecretKey;

        public AdminAuthService(
            IAdminRepository adminRepo,
            IConfiguration config)
        {
            _adminRepo = adminRepo;
            _validSecretKey = config["AdminSecretKey"]; // From appsettings.json
        }

        public async Task<Admin> Register(Admin admin, string password, string secretKey)
        {
            // 1. Validate secret key
            if (secretKey != _validSecretKey)
                throw new UnauthorizedAccessException("Invalid secret key");

            // 2. Check if admin already exists
            if (await _adminRepo.AdminExists(admin.Email))
                throw new InvalidOperationException("Admin already exists");

            // 3. Hash password with enhanced BCrypt algorithm
            admin.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor: 12);

            return await _adminRepo.RegisterAdmin(admin);
        }

        public async Task<Admin> Login(string email, string password)
        {
            // 1. Get admin by email
            var admin = await _adminRepo.GetByEmail(email);

            // 2. Verify credentials
            if (admin == null || !BCrypt.Net.BCrypt.EnhancedVerify(password, admin.Password))
                throw new UnauthorizedAccessException("Invalid email or password");

            return admin;
        }
    }
}