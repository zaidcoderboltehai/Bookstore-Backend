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
        private readonly IUserRepository _userRepo; // User repository ka reference
        private readonly IAdminRepository _adminRepo; // Admin repository ka reference
        private readonly IPasswordResetRepository _resetRepo; // Password reset repository ka reference
        private readonly IConfiguration _config; // Configuration se settings lene ke liye

        // Constructor: Jab ForgotPasswordService banayi jaati hai, toh yeh dependencies inject ki jaati hain
        public ForgotPasswordService(
            IUserRepository userRepo, // User repository ka reference
            IAdminRepository adminRepo, // Admin repository ka reference
            IPasswordResetRepository resetRepo, // Password reset repository ka reference
            IConfiguration config) // Configuration settings ko read karne ke liye
        {
            _userRepo = userRepo; // _userRepo ko initialize kar rahe hain
            _adminRepo = adminRepo; // _adminRepo ko initialize kar rahe hain
            _resetRepo = resetRepo; // _resetRepo ko initialize kar rahe hain
            _config = config; // _config ko initialize kar rahe hain
        }

        // User ke liye password reset link bhejne ki method
        public async Task SendUserForgotPasswordLink(string email)
        {
            if (!await _userRepo.UserExists(email)) // Agar user nahi milta, toh error throw karega
                throw new InvalidOperationException("User not found");

            var token = Guid.NewGuid(); // Unique token generate kar rahe hain
            var reset = new PasswordReset // Password reset ka record banaya jaa raha hai
            {
                Token = token, // Token ko set kiya
                Email = email, // User ka email set kiya
                ExpiryUtc = DateTime.UtcNow.AddHours(1) // Token ka expiry time set kiya
            };
            await _resetRepo.CreateAsync(reset); // Reset record ko database mein save kiya

            var frontUrl = _config["Frontend:ResetPasswordUrl"]; // Frontend URL ko config se liya
            var link = $"{frontUrl}?token={token}"; // Reset password link banayi jaa rahi hai
            // TODO: Email bhejne ka logic implement karna hai yahan
        }

        // Admin ke liye password reset link bhejne ki method
        public async Task SendAdminForgotPasswordLink(string email, string secretKey)
        {
            if (secretKey != _config["AdminSecretKey"]) // Agar secret key match nahi karti, toh error throw karega
                throw new UnauthorizedAccessException("Invalid SecretKey");

            if (!await _adminRepo.AdminExists(email)) // Agar admin nahi milta, toh error throw karega
                throw new InvalidOperationException("Admin not found");

            var token = Guid.NewGuid(); // Unique token generate kar rahe hain
            var reset = new PasswordReset // Password reset record banaya jaa raha hai
            {
                Token = token, // Token ko set kiya
                Email = email, // Admin ka email set kiya
                ExpiryUtc = DateTime.UtcNow.AddHours(1) // Token ka expiry time set kiya
            };
            await _resetRepo.CreateAsync(reset); // Reset record ko database mein save kiya

            var frontUrl = _config["Frontend:ResetPasswordUrl"]; // Frontend URL ko config se liya
            var link = $"{frontUrl}?token={token}"; // Reset password link banayi jaa rahi hai
            // TODO: Email bhejne ka logic implement karna hai yahan
        }

        // Password reset karne ki method
        public async Task ResetPassword(Guid token, string newPassword)
        {
            var record = await _resetRepo.GetByTokenAsync(token) // Token se reset record ko fetch kar rahe hain
                ?? throw new InvalidOperationException("Invalid or expired token"); // Agar record nahi milta, toh error throw karega

            if (record.ExpiryUtc < DateTime.UtcNow) // Agar token expire ho gaya ho, toh error throw karega
                throw new InvalidOperationException("Token expired");

            // Agar user ka record milta hai toh password update karenge
            if (await _userRepo.UserExists(record.Email))
            {
                var user = await _userRepo.GetUserByEmail(record.Email); // User ko email ke through fetch kar rahe hain
                if (user == null) // Agar user nahi milta, toh error throw karega
                    throw new InvalidOperationException("User not found");

                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword); // Naya password hash kar rahe hain
                await _userRepo.UpdateUserAsync(user); // User ka password update kar rahe hain
            }
            else // Agar user nahi hai, toh admin ka password update karenge
            {
                var admin = await _adminRepo.GetByEmail(record.Email); // Admin ko email ke through fetch kar rahe hain
                admin.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword, workFactor: 12); // Admin ka password hash kar rahe hain
                await _adminRepo.UpdateAdminAsync(admin); // Admin ka password update kar rahe hain
            }

            await _resetRepo.DeleteAsync(record); // Reset record ko delete kar rahe hain
        }
    }
}
