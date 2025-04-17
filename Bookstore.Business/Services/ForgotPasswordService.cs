using System;
using System.Threading.Tasks;
using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Bookstore.Business.Services
{
    public class ForgotPasswordService : IForgotPasswordService
    {
        private readonly IUserRepository _userRepo;
        private readonly IAdminRepository _adminRepo;
        private readonly IPasswordResetRepository _resetRepo;
        private readonly IConfiguration _config;

        public ForgotPasswordService(
            IUserRepository userRepo,
            IAdminRepository adminRepo,
            IPasswordResetRepository resetRepo,
            IConfiguration config)
        {
            _userRepo = userRepo;
            _adminRepo = adminRepo;
            _resetRepo = resetRepo;
            _config = config;
        }

        public async Task SendUserForgotPasswordLink(string email)
        {
            if (!await _userRepo.UserExists(email))
                throw new InvalidOperationException("User not found");

            var token = Guid.NewGuid();
            var reset = new PasswordReset
            {
                Token = token,
                Email = email,
                ExpiryUtc = DateTime.UtcNow.AddHours(1)
            };
            await _resetRepo.CreateAsync(reset);

            var frontUrl = _config["Frontend:ResetPasswordUrl"];
            var link = $"{frontUrl}?token={token}";
            // TODO: Send email
        }

        public async Task SendAdminForgotPasswordLink(string email, string secretKey)
        {
            if (secretKey != _config["AdminSecretKey"])
                throw new UnauthorizedAccessException("Invalid SecretKey");
            if (!await _adminRepo.AdminExists(email))
                throw new InvalidOperationException("Admin not found");

            var token = Guid.NewGuid();
            var reset = new PasswordReset
            {
                Token = token,
                Email = email,
                ExpiryUtc = DateTime.UtcNow.AddHours(1)
            };
            await _resetRepo.CreateAsync(reset);

            var frontUrl = _config["Frontend:ResetPasswordUrl"];
            var link = $"{frontUrl}?token={token}";
            // TODO: Send email
        }

        public async Task ResetPassword(Guid token, string newPassword)
        {
            var record = await _resetRepo.GetByTokenAsync(token)
                ?? throw new InvalidOperationException("Invalid or expired token");

            if (record.ExpiryUtc < DateTime.UtcNow)
                throw new InvalidOperationException("Token expired");

            // Check if email belongs to user or admin
            if (await _userRepo.UserExists(record.Email))
            {
                // ✅ Fixed: Use GetUserByEmail instead of LoginUser
                var user = await _userRepo.GetUserByEmail(record.Email);
                if (user == null)
                    throw new InvalidOperationException("User not found");

                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                await _userRepo.UpdateUser(user); // Save updated password
            }
            else
            {
                var admin = await _adminRepo.GetByEmail(record.Email);
                admin.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword, workFactor: 12);
                await _adminRepo.RegisterAdmin(admin); // Save admin (if using UpdateAdminAsync, use that instead)
            }

            await _resetRepo.DeleteAsync(record); // Cleanup the token
        }
    }
}