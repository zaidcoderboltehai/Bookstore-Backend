using System;
using System.Threading.Tasks;
using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BCrypt.Net; // ✅ सही namespace

namespace Bookstore.Business.Services
{
    public class ForgotPasswordService : IForgotPasswordService
    {
        private readonly IUserRepository _userRepo;
        private readonly IAdminRepository _adminRepo;
        private readonly IPasswordResetRepository _resetRepo;
        private readonly IConfiguration _config;
        private readonly ILogger<ForgotPasswordService> _logger;

        public ForgotPasswordService(
            IUserRepository userRepo,
            IAdminRepository adminRepo,
            IPasswordResetRepository resetRepo,
            IConfiguration config,
            ILogger<ForgotPasswordService> logger)
        {
            _userRepo = userRepo;
            _adminRepo = adminRepo;
            _resetRepo = resetRepo;
            _config = config;
            _logger = logger;
        }

        public async Task SendUserForgotPasswordLink(string email)
        {
            try
            {
                _logger.LogInformation("User password reset requested for {Email}", email);

                var user = await _userRepo.GetUserByEmail(email);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {Email}", email);
                    throw new InvalidOperationException("User not found");
                }

                var token = Guid.NewGuid();
                await CreatePasswordResetRecord(email, token);

                var resetLink = GenerateResetLink(token, "user");
                LogResetLink(email, resetLink);

                // TODO: Implement email service
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in user password reset for {Email}", email);
                throw;
            }
        }

        public async Task SendAdminForgotPasswordLink(string email, string secretKey)
        {
            try
            {
                _logger.LogInformation("Admin password reset requested for {Email}", email);

                if (secretKey != _config["AdminSecretKey"])
                {
                    _logger.LogWarning("Invalid secret key attempt for admin: {Email}", email);
                    throw new UnauthorizedAccessException("Invalid admin credentials");
                }

                var admin = await _adminRepo.GetByEmail(email);
                if (admin == null)
                {
                    _logger.LogWarning("Admin not found: {Email}", email);
                    throw new InvalidOperationException("Admin not found");
                }

                var token = Guid.NewGuid();
                await CreatePasswordResetRecord(email, token);

                var resetLink = GenerateResetLink(token, "admin");
                LogResetLink(email, resetLink);

                // TODO: Implement email service
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in admin password reset for {Email}", email);
                throw;
            }
        }

        public async Task ResetPassword(Guid token, string newPassword)
        {
            try
            {
                _logger.LogInformation("Processing password reset for token: {Token}", token);

                var record = await _resetRepo.GetByTokenAsync(token)
                    ?? throw new InvalidOperationException("Invalid token");

                if (record.ExpiryUtc < DateTime.UtcNow)
                {
                    _logger.LogWarning("Expired token: {Token}", token);
                    throw new InvalidOperationException("Token expired");
                }

                await UpdatePassword(record.Email, newPassword);
                await _resetRepo.DeleteAsync(record);

                _logger.LogInformation("Password reset successful for {Email}", record.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset failed for token: {Token}", token);
                throw;
            }
        }

        private async Task CreatePasswordResetRecord(string email, Guid token)
        {
            await _resetRepo.CreateAsync(new PasswordReset
            {
                Token = token,
                Email = email,
                ExpiryUtc = DateTime.UtcNow.AddHours(1)
            });
        }

        private string GenerateResetLink(Guid token, string userType)
        {
            var baseUrl = _config["Frontend:BaseUrl"];
            return userType.ToLower() switch
            {
                "admin" => $"{baseUrl}/admin-reset-password?token={token}",
                _ => $"{baseUrl}/reset-password?token={token}"
            };
        }

        private async Task UpdatePassword(string email, string newPassword)
        {
            var user = await _userRepo.GetUserByEmail(email);
            if (user != null)
            {
                // ✅ BCrypt.Net-Next के साथ EnhancedHashPassword
                user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword, workFactor: 12);
                await _userRepo.UpdateUserAsync(user);
                return;
            }

            var admin = await _adminRepo.GetByEmail(email);
            if (admin != null)
            {
                admin.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword, workFactor: 12);
                await _adminRepo.UpdateAdminAsync(admin);
                return;
            }

            throw new InvalidOperationException("User not found");
        }

        private void LogResetLink(string email, string link)
        {
            _logger.LogDebug("Password Reset Link for {Email}: {Link}", email, link);
        }
    }
}