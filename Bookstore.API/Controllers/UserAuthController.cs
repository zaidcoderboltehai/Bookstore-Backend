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
    [ApiController]
    [Route("Users")] // ✅ Fixed route to match frontend calls
    [Authorize(Roles = "User,ADMIN")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repo;
        private readonly IUserAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IForgotPasswordService _forgotService;
        private readonly IRefreshTokenRepository _refreshTokenRepo;
        private readonly IHostEnvironment _environment;

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

        // ---------------------- Get All Users ----------------------
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _repo.GetAllUsersAsync();
            return Ok(new
            {
                Count = users.Count(),
                Results = users.Select(u => new { u.Id, u.Email, u.Role })
            });
        }

        // ---------------------- User Registration ----------------------
        [HttpPost("register")]
        [AllowAnonymous]
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
                return Ok(new { Status = "Success", UserId = user.Id });
            }
            catch (Exception ex)
            {
                AuditLog($"Registration failed: {ex.Message}");
                return Conflict(new { Error = "RegistrationFailed", Message = ex.Message });
            }
        }

        // ---------------------- User Login ----------------------
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            try
            {
                AuditLog($"Login attempt: {dto.Email}");
                var user = await _authService.Login(dto.Email, dto.Password);

                var accessToken = _tokenService.CreateToken(user);
                var refreshToken = _tokenService.GenerateRefreshToken();

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
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = 1800,
                    UserId = user.Id
                });
            }
            catch (Exception ex)
            {
                AuditLog($"Login failed: {dto.Email} - {ex.Message}");
                return Unauthorized(new { Error = "LoginFailed", Message = "Invalid email/password" });
            }
        }

        // ---------------------- Refresh Token ----------------------
        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
                var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);

                var storedToken = await _refreshTokenRepo.FindByTokenAsync(request.RefreshToken);
                if (storedToken == null || storedToken.UserId != userId || storedToken.Expires < DateTime.UtcNow)
                    return BadRequest(new { Error = "InvalidToken", Message = "Invalid/expired refresh token" });

                var newAccessToken = _tokenService.CreateTokenFromClaims(principal.Claims);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                await _refreshTokenRepo.DeleteAsync(storedToken.Id);
                await _refreshTokenRepo.CreateAsync(new RefreshToken
                {
                    Token = newRefreshToken,
                    Expires = DateTime.UtcNow.AddDays(7),
                    UserId = userId,
                    UserType = "User"
                });

                return Ok(new
                {
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresIn = 1800
                });
            }
            catch (Exception ex)
            {
                AuditLog($"Token refresh failed: {ex.Message}");
                return StatusCode(500, new { Error = "TokenRefreshFailed", Message = ex.Message });
            }
        }

        // ---------------------- Forgot Password ----------------------
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            try
            {
                await _forgotService.SendUserForgotPasswordLink(dto.Email);
                AuditLog($"Password reset link sent to: {dto.Email}");
                return Ok(new { Status = "ResetLinkSent" });
            }
            catch (Exception ex)
            {
                AuditLog($"Forgot password failed: {dto.Email} - {ex.Message}");
                return BadRequest(new { Error = "ForgotPasswordFailed", Message = ex.Message });
            }
        }

        // ---------------------- Reset Password ----------------------
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                await _forgotService.ResetPassword(dto.Token, dto.NewPassword);
                AuditLog($"Password reset successful for token: {dto.Token}");
                return Ok(new { Status = "PasswordResetSuccess" });
            }
            catch (Exception ex)
            {
                AuditLog($"Password reset failed: {dto.Token} - {ex.Message}");
                return BadRequest(new { Error = "PasswordResetFailed", Message = ex.Message });
            }
        }

        // ---------------------- Helper Method ----------------------
        private void AuditLog(string message)
        {
            Console.WriteLine($"[AUDIT] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} - {message}");
        }
    }
}