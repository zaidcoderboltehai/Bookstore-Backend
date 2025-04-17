using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Business.Services;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Bookstore.API.Models;
using Bookstore.Business.Interfaces;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Bookstore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "User,Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repo;
        private readonly IUserAuthService _authService;
        private readonly TokenService _tokenService;
        private readonly IForgotPasswordService _forgotService;

        public UsersController(
            IUserRepository repo,
            IUserAuthService authService,
            TokenService tokenService,
            IForgotPasswordService forgotService)
        {
            _repo = repo;
            _authService = authService;
            _tokenService = tokenService;
            _forgotService = forgotService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _repo.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _repo.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    Role = dto.Role
                };

                var createdUser = await _authService.Register(user, dto.Password);
                var token = _tokenService.CreateToken(createdUser);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var user = await _authService.Login(dto.Email, dto.Password);
                var token = _tokenService.CreateToken(user);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _forgotService.SendUserForgotPasswordLink(dto.Email);
            return Ok("Password reset link sent if email exists.");
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _forgotService.ResetPassword(dto.Token, dto.NewPassword);
            return Ok("Password has been reset.");
        }
    }
}
