using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Business.Services;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Bookstore.API.Models;
using Bookstore.Business.Interfaces;
using System.Threading.Tasks;
using System;
using System.Security.Cryptography;
using Bookstore.Data;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace Bookstore.API.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("[controller]")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IAdminAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IForgotPasswordService _forgotService;
        private readonly IRefreshTokenRepository _refreshTokenRepo;

        public AdminAuthController(
            IAdminAuthService authService,
            ITokenService tokenService,
            IForgotPasswordService forgotService,
            IRefreshTokenRepository refreshTokenRepo)
        {
            _authService = authService;
            _tokenService = tokenService;
            _forgotService = forgotService;
            _refreshTokenRepo = refreshTokenRepo;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(AdminRegisterDto request)
        {
            var admin = new Admin
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                SecretKey = request.SecretKey
            };

            await _authService.Register(admin, request.Password, request.SecretKey);
            return Ok(new
            {
                message = "Admin registration successful",
                nextSteps = "Proceed to login with your credentials"
            });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(AdminLoginDto request)
        {
            try
            {
                var admin = await _authService.Login(request.Email, request.Password);
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
                    message = "Admin login successful",
                    accessToken,
                    refreshToken,
                    expiresIn = 1800 // 30 minutes in seconds
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new
                {
                    error = "Authentication failed",
                    details = ex.Message
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto)
        {
            try
            {
                await _forgotService.SendAdminForgotPasswordLink(dto.Email, dto.SecretKey);
                return Ok(new
                {
                    message = "Password reset instructions sent if account exists",
                    securityNote = "Check spam folder if not received within 5 minutes"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = "Password reset failed",
                    details = ex.Message
                });
            }
        }

        // ✅ Added Reset Password Endpoint
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
                    Message = "Admin password updated successfully",
                    NextSteps = "Login with new credentials"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    ErrorCode = "InvalidTokenOrSecret",
                    Details = ex.Message
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
                var adminId = int.Parse(principal.FindFirst(JwtRegisteredClaimNames.Sub).Value);

                var storedToken = await _refreshTokenRepo.FindByTokenAsync(request.RefreshToken);

                if (storedToken == null || storedToken.UserId != adminId || storedToken.IsExpired)
                    throw new SecurityTokenException("Invalid refresh token");

                var newAccessToken = _tokenService.CreateToken(principal.Claims);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                // Update tokens
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
                    message = "Tokens refreshed successfully",
                    accessToken = newAccessToken,
                    refreshToken = newRefreshToken
                });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new
                {
                    error = "Token validation failed",
                    details = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Token refresh error",
                    details = ex.Message
                });
            }
        }
    }
}