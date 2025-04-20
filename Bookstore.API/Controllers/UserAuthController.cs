// Required namespaces for controller, auth, models, interfaces, etc.
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Bookstore.API.Models;
using Bookstore.Business.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Bookstore.API.Controllers
{
    // Ye controller API ke liye hai
    [ApiController]
    [Route("[controller]")]
    // Sirf User ya Admin role wale hi access kar sakte hai
    [Authorize(Roles = "User,Admin")]
    public class UsersController : ControllerBase
    {
        // Dependencies ko inject kiya gaya hai
        private readonly IUserRepository _repo;
        private readonly IUserAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IForgotPasswordService _forgotService;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IHostEnvironment _environment;

        // Constructor - yahan sab services inject ho rahe hai
        public UsersController(
            IUserRepository repo,
            IUserAuthService authService,
            ITokenService tokenService,
            IForgotPasswordService forgotService,
            IRefreshTokenRepository refreshTokenRepo,
            IHostEnvironment environment)
        {
            _repo = repo;
            _authService = authService;
            _tokenService = tokenService;
            _forgotService = forgotService;
            _refreshTokenRepo = refreshTokenRepo;
            _environment = environment;
        }

        // Sabhi users ki list laata hai
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _repo.GetAllUsersAsync();
            return Ok(new
            {
                Count = users.Count(),
                Results = users.Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.Role,
                    Name = $"{u.FirstName} {u.LastName}"
                })
            });
        }

        // Kisi specific user ko ID se laata hai
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _repo.GetUserByIdAsync(id);
            if (user == null) return NotFound(new
            {
                Status = "NotFound",
                Error = "User not found",
                Id = id
            });

            return Ok(new
            {
                User = new
                {
                    user.Id,
                    user.Email,
                    user.Role,
                    user.FirstName,
                    user.LastName
                },
                Links = new
                {
                    // Self-link return karta hai (HATEOAS approach)
                    Self = Url.ActionLink(
                        action: nameof(GetById),
                        controller: "Users",
                        values: new { id },
                        protocol: Request.Scheme
                    )!
                }
            });
        }

        // New user registration
        [HttpPost("register")]
        [AllowAnonymous] // Publicly accessible hai
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            try
            {
                var user = new User
                {
                    FirstName = dto.FirstName.Trim(),
                    LastName = dto.LastName.Trim(),
                    Email = dto.Email.ToLower().Trim(),
                    Role = string.IsNullOrEmpty(dto.Role) ? "User" : dto.Role
                };

                await _authService.Register(user, dto.Password);
                AuditLog($"User registered: {user.Email}");

                return Ok(new
                {
                    Status = "Success",
                    UserId = user.Id,
                    NextSteps = new[] {
                        "Check email for verification",
                        "Login with credentials"
                    }
                });
            }
            catch (Exception ex)
            {
                AuditLog($"Registration failed: {ex.Message}");
                return Conflict(new
                {
                    Status = "Error",
                    ErrorType = "RegistrationError",
                    UserMessage = "Registration failed. Please check your details.",
                    Technical = _environment.IsDevelopment() ? ex.Message : null,
                    ErrorCode = "USER-REG-100"
                });
            }
        }

        // User login endpoint
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            try
            {
                AuditLog($"Login attempt: {dto.Email}");
                var user = await _authService.Login(dto.Email, dto.Password);

                // Token generate karta hai
                var accessToken = _tokenService.CreateToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Refresh token DB me save hota hai
                await _refreshTokenRepo.CreateAsync(new RefreshToken
                {
                    Token = refreshToken,
                    Expires = DateTime.UtcNow.AddDays(7),
                    UserId = user.Id,
                    UserType = "User"
                });

                AuditLog($"Successful login: {user.Id}");

                return Ok(new
                {
                    Status = "Authenticated",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = 1800, // 30 mins
                    UserInfo = new
                    {
                        user.Id,
                        user.Email,
                        user.Role,
                        FullName = $"{user.FirstName} {user.LastName}"
                    }
                });
            }
            catch (Exception ex)
            {
                AuditLog($"Login failed: {dto.Email} - {ex.Message}");
                return Unauthorized(new
                {
                    Status = "AuthFailed",
                    UserMessage = "Invalid email/password",
                    DebugInfo = _environment.IsDevelopment() ? ex.Message : null,
                    ErrorCode = "USER-AUTH-200"
                });
            }
        }

        // Refresh token endpoint - naya token issue karta hai
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                AuditLog($"Refresh token attempt from: {Request.HttpContext.Connection.RemoteIpAddress}");

                var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);

                // Token me user type check karta hai
                var userTypeClaim = principal.FindFirst("UserType")?.Value;
                if (userTypeClaim != "User")
                {
                    AuditLog($"Invalid user type attempt: {userTypeClaim}");
                    throw new SecurityTokenException("Invalid token type for user");
                }

                // Token se userId nikaal ke check karta hai
                var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub) ??
                                principal.FindFirst(ClaimTypes.NameIdentifier);

                if (!int.TryParse(userIdClaim?.Value, out var userId))
                {
                    AuditLog("Invalid user ID format in token");
                    throw new SecurityTokenException("Invalid user identifier");
                }

                // Refresh token validate karta hai
                var storedToken = await _refreshTokenRepo.FindByTokenAsync(request.RefreshToken);
                if (storedToken == null ||
                    storedToken.UserId != userId ||
                    storedToken.UserType != "User" ||
                    storedToken.Expires < DateTime.UtcNow)
                {
                    AuditLog($"Invalid refresh token for user {userId}");
                    throw new SecurityTokenException("Invalid or expired refresh token");
                }

                // Token rotate karta hai (naye tokens deta hai)
                var newAccessToken = _tokenService.CreateToken(principal.Claims);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                await _refreshTokenRepo.DeleteAsync(storedToken.Id);
                await _refreshTokenRepo.CreateAsync(new RefreshToken
                {
                    Token = newRefreshToken,
                    Expires = DateTime.UtcNow.AddDays(7),
                    UserId = userId,
                    UserType = "User"
                });

                AuditLog($"Tokens refreshed for user {userId}");

                return Ok(new
                {
                    Status = "TokenRefreshed",
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = 1800,
                    UserInfo = new
                    {
                        UserId = userId,
                        UserType = "User"
                    }
                });
            }
            catch (SecurityTokenException ex)
            {
                AuditLog($"Token error: {ex.Message}");
                return Unauthorized(new
                {
                    Status = "TokenError",
                    ErrorCode = "TOKEN-1001",
                    UserMessage = "Invalid token",
                    TechnicalDetails = _environment.IsDevelopment() ? ex.Message : null
                });
            }
            catch (Exception ex)
            {
                AuditLog($"Refresh token failure: {ex}");
                return StatusCode(500, new
                {
                    Status = "ServerError",
                    ErrorCode = "SRV-1001",
                    UserMessage = "Token refresh failed",
                    TechnicalDetails = _environment.IsDevelopment() ? ex.Message : null
                });
            }
        }

        // Forgot password email bhejne ka endpoint
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            try
            {
                await _forgotService.SendUserForgotPasswordLink(dto.Email);
                AuditLog($"Password reset requested: {dto.Email}");

                return Ok(new
                {
                    Status = "InstructionsSent",
                    Validity = "1 hour"
                });
            }
            catch (Exception ex)
            {
                AuditLog($"Password reset failed: {dto.Email} - {ex.Message}");
                return NotFound(new
                {
                    Status = "NotFound",
                    UserMessage = "Email not registered",
                    Technical = _environment.IsDevelopment() ? ex.Message : null,
                    Solution = "Check email or register"
                });
            }
        }

        // Password reset hone ka endpoint
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                await _forgotService.ResetPassword(dto.Token, dto.NewPassword);
                AuditLog($"Password reset successful for token: {dto.Token}");

                return Ok(new
                {
                    Status = "PasswordReset",
                    Message = "Password updated successfully",
                    NextSteps = "Login with new credentials"
                });
            }
            catch (Exception ex)
            {
                AuditLog($"Password reset failed: {dto.Token} - {ex.Message}");
                return BadRequest(new
                {
                    Status = "Error",
                    ErrorCode = "TOKEN-1002",
                    UserMessage = "Invalid reset token",
                    TechnicalDetails = _environment.IsDevelopment() ? ex.Message : null
                });
            }
        }

        // Console pe log print karta hai for debugging and audit
        private void AuditLog(string message)
        {
            Console.WriteLine($"[AUDIT] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
        }
    }
}
