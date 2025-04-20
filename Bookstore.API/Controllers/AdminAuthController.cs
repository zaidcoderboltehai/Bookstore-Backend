// Required namespaces import kiye gaye hain
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
    // Isse bataya gaya hai ki ye ek API controller hai
    [ApiController]

    // API ka route set kiya gaya hai: /AdminAuth
    [Route("[controller]")]

    // Sirf Admin role wale hi access kar sakte hain
    [Authorize(Roles = "Admin")]
    public class AdminAuthController : ControllerBase
    {
        // Private fields jo constructor ke through initialize hote hain
        private readonly IAdminAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IHostEnvironment _environment;
        private readonly ILogger<AdminAuthController> _logger;

        // Constructor dependency injection ke through sab services set karta hai
        public AdminAuthController(
            IAdminAuthService authService,
            ITokenService tokenService,
            IRefreshTokenRepository refreshTokenRepo,
            IHostEnvironment environment,
            ILogger<AdminAuthController> logger)
        {
            _authService = authService;
            _tokenService = tokenService;
            _refreshTokenRepo = refreshTokenRepo;
            _environment = environment;
            _logger = logger;
        }

        // Admin ko register karne ke liye API
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(AdminRegisterDto request)
        {
            try
            {
                // Logging: register ka try kiya gaya
                _logger.LogInformation("Admin registration attempt for {Email}", request.Email);

                // Naya admin object banaya gaya
                var admin = new Admin
                {
                    FirstName = request.FirstName.Trim(),
                    LastName = request.LastName.Trim(),
                    Email = request.Email.ToLower().Trim(),
                    SecretKey = request.SecretKey
                };

                // Auth service ke through register call kiya
                var createdAdmin = await _authService.Register(admin, request.Password, request.SecretKey);

                // Logging: registration success
                _logger.LogInformation("Admin registered successfully: {AdminId}", createdAdmin.Id);

                // Response with success details
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
                // Error log aur client ko message
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

        // Admin login karne ke liye API
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(AdminLoginDto request)
        {
            try
            {
                _logger.LogInformation("Login attempt from: {Email}", request.Email);

                // Login service call
                var admin = await _authService.Login(request.Email.Trim().ToLower(), request.Password);
                _logger.LogInformation("Successful login for admin ID: {AdminId}", admin.Id);

                // Token generate karna
                var accessToken = _tokenService.CreateToken(admin);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Refresh token DB me save karna
                await _refreshTokenRepo.CreateAsync(new RefreshToken
                {
                    Token = refreshToken,
                    Expires = DateTime.UtcNow.AddDays(7),
                    UserId = admin.Id,
                    UserType = "Admin"
                });
                _logger.LogInformation("Refresh token saved for admin {AdminId}", admin.Id);

                // Response with token and user info
                return Ok(new
                {
                    Status = "Success",
                    Data = new
                    {
                        AdminInfo = new { admin.Id, admin.Email },
                        Tokens = new { accessToken, refreshToken, ExpiresIn = 1800 } // 30 mins
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

        // Token refresh karne ke liye API
        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                _logger.LogInformation("Admin token refresh attempt");

                // Expired token se user ka claim nikalna
                var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);

                // UserType check karna (sirf Admin allowed)
                var userTypeClaim = principal.FindFirst("UserType")?.Value;
                if (userTypeClaim != "Admin")
                {
                    _logger.LogWarning("Invalid user type for admin refresh: {UserType}", userTypeClaim);
                    throw new SecurityTokenException("Invalid token type for admin");
                }

                // Admin ID nikalna token se
                var adminIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub) ??
                                 principal.FindFirst(ClaimTypes.NameIdentifier);

                if (adminIdClaim == null || !int.TryParse(adminIdClaim.Value, out var adminId))
                {
                    _logger.LogWarning("Invalid token: Missing or invalid admin identifier");
                    throw new SecurityTokenException("Invalid admin identifier in token");
                }

                // Refresh token validate karna
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

                // New token banana (token rotation)
                var newAccessToken = _tokenService.CreateToken(principal.Claims);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                // Old refresh token delete karna aur naya add karna
                await _refreshTokenRepo.DeleteAsync(storedToken.Id);
                await _refreshTokenRepo.CreateAsync(new RefreshToken
                {
                    Token = newRefreshToken,
                    Expires = DateTime.UtcNow.AddDays(7),
                    UserId = adminId,
                    UserType = "Admin"
                });

                _logger.LogInformation("Tokens refreshed successfully for admin {AdminId}", adminId);

                // Success response
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
    }
}
