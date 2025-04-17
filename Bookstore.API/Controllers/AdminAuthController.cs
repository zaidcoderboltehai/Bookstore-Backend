using Microsoft.AspNetCore.Authorization; // Add this line
using Bookstore.Business.Services;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Bookstore.API.Models;
using Bookstore.Business.Interfaces;
using System.Threading.Tasks;
using System;

namespace Bookstore.API.Controllers
{
    [Authorize(Roles = "Admin")] // 👈 Admin role required for all endpoints
    [ApiController]
    [Route("api/[controller]")]
    public class AdminAuthController : ControllerBase
    {
        private readonly IAdminAuthService _authService;
        private readonly TokenService _tokenService;
        private readonly IForgotPasswordService _forgotService;

        public AdminAuthController(
            IAdminAuthService authService,
            TokenService tokenService,
            IForgotPasswordService forgotService)
        {
            _authService = authService;
            _tokenService = tokenService;
            _forgotService = forgotService;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(AdminRegisterDto request)
        {
            // no try/catch: let global dev exception page show full error
            var admin = new Admin
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                SecretKey = request.SecretKey
            };

            var createdAdmin = await _authService.Register(
                admin,
                request.Password,
                request.SecretKey
            );

            var token = _tokenService.CreateToken(createdAdmin);
            return Ok(new { token });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(AdminLoginDto request)
        {
            try
            {
                var admin = await _authService.Login(request.Email, request.Password);
                var token = _tokenService.CreateToken(admin);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordRequestDto dto)
        {
            await _forgotService.SendAdminForgotPasswordLink(dto.Email, dto.SecretKey!);
            return Ok("Password reset link sent if admin exists and secret key valid.");
        }
    }
}
