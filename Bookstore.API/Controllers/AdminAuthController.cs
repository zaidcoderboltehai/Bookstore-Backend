using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Bookstore.API.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Bookstore.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IAdminAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<AdminAuthController> _logger;
        private readonly IForgotPasswordService _forgotService;

        public AdminAuthController(
            IAdminAuthService authService,
            ITokenService tokenService,
            IRefreshTokenRepository refreshTokenRepo,
            IHostEnvironment environment,
            ILogger<AdminAuthController> logger,
            IForgotPasswordService forgotService)
        {
            _authService = authService;
            _tokenService = tokenService;
            _refreshTokenRepo = refreshTokenRepo;
            _environment = environment;
            _logger = logger;
            _forgotService = forgotService;
        }

        // ✅ Admin Registration with ExternalId for CSV
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(AdminRegisterDto request)
        {
            try
            {
                _logger.LogInformation("Admin registration attempt for {Email}", request.Email);

                var admin = new Admin
                {
                    ExternalId = Guid.NewGuid().ToString(), // Auto-generated GUID
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = request.Email.ToLower().Trim(),
                    SecretKey = request.SecretKey,
                    Role = "Admin"
                };

                var createdAdmin = await _authService.Register(admin, request.Password, request.SecretKey);
                _logger.LogInformation("Admin registered: ID {AdminId}", createdAdmin.Id);

                return Ok(new
                {
                    Status = "Success",
                    Admin = new
                    {
                        createdAdmin.Id,
                        createdAdmin.ExternalId, // Return for CSV use
                        createdAdmin.Email,
                        createdAdmin.FirstName,
                        createdAdmin.LastName
                    },
                    Message = "Admin created. Store ExternalId for CSV operations."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin registration failed");
                return BadRequest(new
                {
                    Status = "Error",
                    UserMessage = "Check secret key/email format",
                    ErrorCode = "ADMIN-REG-100",
                    Debug = _environment.IsDevelopment() ? ex.Message : null
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(AdminLoginDto request)
        {
            try
            {
                _logger.LogInformation("Login attempt: {Email}", request.Email);

                var admin = await _authService.Login(request.Email.Trim().ToLower(), request.Password);

                var accessToken = _tokenService.CreateToken(admin);
                var refreshToken = _tokenService.GenerateRefreshToken();

                await _refreshTokenRepo.CreateAsync(new RefreshToken
                {
                    Token = refreshToken,
                    Expires = DateTime.UtcNow.AddDays(7),
                    UserId = admin.Id,
                    UserType = "Admin"
                });

                return Ok(new
                {
                    Status = "AuthSuccess",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = 1800, // 30 minutes
                    AdminId = admin.Id
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Email}", request.Email);
                return Unauthorized(new
                {
                    Status = "AuthFailed",
                    ErrorCode = "ADMIN-LOGIN-200",
                    Message = "Invalid email/password"
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
                var userType = principal.FindFirst("UserType")?.Value;

                if (userType != "Admin")
                    throw new SecurityTokenException("Invalid token type");

                var adminIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(adminIdClaim, out int adminId))
                    throw new SecurityTokenException("Invalid admin ID");

                var storedToken = await _refreshTokenRepo.FindByTokenAsync(request.RefreshToken);
                if (storedToken == null || storedToken.UserId != adminId || storedToken.IsExpired)
                    throw new SecurityTokenException("Invalid/expired refresh token");

                // Generate new tokens
                var newAccessToken = _tokenService.CreateToken(principal.Claims);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                // Update refresh token
                await _refreshTokenRepo.DeleteAsync(storedToken.Id);
                await _refreshTokenRepo.CreateAsync(new RefreshToken
                {
                    Token = newRefreshToken,
                    Expires = DateTime.UtcNow.AddDays(7),
                    UserId = adminId,
                    UserType = "Admin"
                });

                return Ok(new
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = 1800
                });
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "Token refresh failed");
                return Unauthorized(new
                {
                    Status = "TokenError",
                    ErrorCode = "TOKEN-100",
                    Message = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            try
            {
                await _forgotService.SendAdminForgotPasswordLink(dto.Email, dto.SecretKey);
                return Ok(new
                {
                    Status = "InstructionsSent",
                    Message = "Check email for reset link (valid 1h)"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset failed");
                return StatusCode(500, new
                {
                    Status = "ServerError",
                    Message = "Internal error. Try later."
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                await _forgotService.ResetPassword(dto.Token, dto.NewPassword);
                return Ok(new
                {
                    Status = "PasswordReset",
                    Message = "Login with new password"
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Password reset error");
                return StatusCode(500, new
                {
                    Status = "ServerError",
                    Message = "Reset failed. Contact support."
                });
            }
        }
    }
}