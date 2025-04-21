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

        // Constructor with Dependency Injection
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

        // Admin Registration Endpoint
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(AdminRegisterDto request)
        {
            try
            {
                _logger.LogInformation("Admin registration attempt for {Email}", request.Email);

                var admin = new Admin
                {
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = request.Email.ToLower().Trim(),
                    SecretKey = request.SecretKey
                };

                var createdAdmin = await _authService.Register(admin, request.Password, request.SecretKey);
                _logger.LogInformation("Admin registered successfully: {AdminId}", createdAdmin.Id);

                return Ok(new
                {
                    Status = "Success",
                    AdminDetails = new
                    {
                        createdAdmin.Id,
                        createdAdmin.FirstName,
                        createdAdmin.LastName,
                        createdAdmin.Email,
                        Role = "Admin"
                    },
                    Message = "Admin account created successfully",
                    NextSteps = new[] { "Check email for verification", "Login with credentials" }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin registration failed for {Email}", request.Email);
                return BadRequest(new
                {
                    Status = "Error",
                    UserMessage = "Registration failed. Check inputs.",
                    TechnicalDetails = _environment.IsDevelopment() ? ex.Message : null,
                    ErrorCode = "REG-1001"
                });
            }
        }

        // Admin Login Endpoint
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(AdminLoginDto request)
        {
            try
            {
                _logger.LogInformation("Login attempt from: {Email}", request.Email);

                var admin = await _authService.Login(request.Email.Trim().ToLower(), request.Password);
                _logger.LogInformation("Successful login for admin ID: {AdminId}", admin.Id);

                var accessToken = _tokenService.CreateToken(admin);
                var refreshToken = _tokenService.GenerateRefreshToken();

                await _refreshTokenRepo.CreateAsync(new RefreshToken
                {
                    Token = refreshToken,
                    Expires = DateTime.UtcNow.AddDays(7),
                    UserId = admin.Id,
                    UserType = "Admin"
                });
                _logger.LogInformation("Refresh token saved for admin {AdminId}", admin.Id);

                return Ok(new
                {
                    Status = "Success",
                    Data = new
                    {
                        AdminInfo = new { admin.Id, admin.Email },
                        Tokens = new { accessToken, refreshToken, ExpiresIn = 1800 }
                    },
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Email}", request.Email);
                return Unauthorized(new
                {
                    Status = "AuthError",
                    UserMessage = "Invalid credentials",
                    ErrorCode = "AUTH-1001"
                });
            }
        }

        // Refresh Token Endpoint
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Admin token refresh attempt");

                var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);

                var userTypeClaim = principal.FindFirst("UserType")?.Value;
                if (userTypeClaim != "Admin")
                {
                    _logger.LogWarning("Invalid user type for admin refresh: {UserType}", userTypeClaim);
                    throw new SecurityTokenException("Invalid token type for admin");
                }

                var adminIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub) ??
                                 principal.FindFirst(ClaimTypes.NameIdentifier);

                if (adminIdClaim == null || !int.TryParse(adminIdClaim.Value, out var adminId))
                {
                    _logger.LogWarning("Invalid token: Missing or invalid admin identifier");
                    throw new SecurityTokenException("Invalid admin identifier in token");
                }

                var storedToken = await _refreshTokenRepo.FindByTokenAsync(request.RefreshToken);
                _logger.LogDebug("Refresh token validation: {TokenId} | User: {AdminId} | Type: {UserType} | Expires: {Expires}",
                    storedToken?.Id, storedToken?.UserId, storedToken?.UserType, storedToken?.Expires);

                if (storedToken == null ||
                    storedToken.UserId != adminId ||
                    storedToken.UserType != "Admin" ||
                    storedToken.Expires < DateTime.UtcNow)
                {
                    _logger.LogWarning("Invalid refresh token for admin {AdminId}", adminId);
                    throw new SecurityTokenException("Invalid or expired refresh token");
                }

                var newAccessToken = _tokenService.CreateToken(principal.Claims);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                await _refreshTokenRepo.DeleteAsync(storedToken.Id);
                await _refreshTokenRepo.CreateAsync(new RefreshToken
                {
                    Token = newRefreshToken,
                    Expires = DateTime.UtcNow.AddDays(7),
                    UserId = adminId,
                    UserType = "Admin"
                });

                _logger.LogInformation("Tokens refreshed successfully for admin {AdminId}", adminId);

                return Ok(new
                {
                    Status = "TokenRefreshed",
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = 1800
                });
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "Token validation failed");
                return Unauthorized(new
                {
                    Status = "TokenError",
                    UserMessage = ex.Message,
                    ErrorCode = "TOKEN-1001"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token refresh failure");
                return StatusCode(500, new
                {
                    Status = "ServerError",
                    UserMessage = "Refresh failed",
                    TechnicalDetails = _environment.IsDevelopment() ? ex.Message : null,
                    ErrorCode = "SRV-1001"
                });
            }
        }

        // Forgot Password Endpoint
        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            try
            {
                _logger.LogInformation("Admin password reset requested for {Email}", dto.Email);

                await _forgotService.SendAdminForgotPasswordLink(dto.Email, dto.SecretKey);

                return Ok(new
                {
                    Status = "InstructionsSent",
                    Validity = "1 hour",
                    Message = "Check admin email for reset instructions"
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Invalid secret key for admin password reset");
                return Unauthorized(new
                {
                    Status = "AuthError",
                    ErrorCode = "ADMIN-AUTH-300",
                    UserMessage = "Invalid admin credentials"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Admin not found for password reset");
                return NotFound(new
                {
                    Status = "NotFound",
                    ErrorCode = "ADMIN-AUTH-301",
                    UserMessage = "Admin account not found"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin password reset failure");
                return StatusCode(500, new
                {
                    Status = "ServerError",
                    ErrorCode = "ADMIN-AUTH-302",
                    UserMessage = "Password reset failed"
                });
            }
        }

        // Reset Password Endpoint
        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                _logger.LogInformation("Admin password reset attempt for token: {Token}", dto.Token);

                await _forgotService.ResetPassword(dto.Token, dto.NewPassword);

                return Ok(new
                {
                    Status = "PasswordReset",
                    Message = "Admin password updated successfully",
                    NextSteps = new[] { "Login with new credentials" }
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid reset token for admin");
                return BadRequest(new
                {
                    Status = "TokenError",
                    ErrorCode = "ADMIN-AUTH-400",
                    UserMessage = "Invalid or expired reset token"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin password reset error");
                return StatusCode(500, new
                {
                    Status = "ServerError",
                    ErrorCode = "ADMIN-AUTH-401",
                    UserMessage = "Password reset failed"
                });
            }
        }
    }
}