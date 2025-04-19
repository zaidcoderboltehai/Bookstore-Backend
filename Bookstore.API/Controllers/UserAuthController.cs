using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Bookstore.API.Models;
using Bookstore.Business.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System;

namespace Bookstore.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "User,Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _repo;
        private readonly IUserAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IForgotPasswordService _forgotService;
        private readonly IRefreshTokenRepository _refreshTokenRepo;

        public UsersController(
            IUserRepository repo,
            IUserAuthService authService,
            ITokenService tokenService,
            IForgotPasswordService forgotService,
            IRefreshTokenRepository refreshTokenRepo)
        {
            _repo = repo;
            _authService = authService;
            _tokenService = tokenService;
            _forgotService = forgotService;
            _refreshTokenRepo = refreshTokenRepo;
        }

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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _repo.GetUserByIdAsync(id);
            if (user == null) return NotFound(new
            {
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
                    Self = Url.ActionLink(
                        action: nameof(GetById),
                        controller: "Users",
                        values: new { id },
                        protocol: Request.Scheme
                    )!
                }
            });
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors) });

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
                return Ok(new
                {
                    Status = "Success",
                    Message = "Registration successful",
                    NextSteps = "Proceed to login"
                });
            }
            catch (Exception ex)
            {
                return Conflict(new
                {
                    Status = "Error",
                    ErrorCode = "RegistrationFailed",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] UserLoginDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { Errors = ModelState.Values.SelectMany(v => v.Errors) });

            try
            {
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

                return Ok(new
                {
                    Status = "Authenticated",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = 1800,
                    UserInfo = new
                    {
                        user.Id,
                        user.Email,
                        user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new
                {
                    Status = "Unauthorized",
                    ErrorCode = "InvalidCredentials",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("refresh-token")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
                var userId = int.Parse(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);

                var storedToken = await _refreshTokenRepo.FindByTokenAsync(request.RefreshToken);
                if (storedToken == null || storedToken.UserId != userId || storedToken.IsExpired)
                    throw new SecurityTokenException("Invalid token combination");

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
                return Unauthorized(new
                {
                    Status = "InvalidToken",
                    ErrorCode = "TokenValidationFailed",
                    Details = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    ErrorCode = "TokenRefreshFailed",
                    Details = ex.Message
                });
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _forgotService.SendUserForgotPasswordLink(dto.Email);
                return Ok(new
                {
                    Status = "InstructionsSent",
                    Message = "Check email for reset link",
                    Validity = "1 hour"
                });
            }
            catch (Exception ex)
            {
                return NotFound(new
                {
                    Status = "Error",
                    ErrorCode = "EmailNotFound",
                    Details = ex.Message
                });
            }
        }

        // ✅ New Password Set करना
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _forgotService.ResetPassword(dto.Token, dto.NewPassword);
                return Ok(new
                {
                    Status = "PasswordReset",
                    Message = "Password updated successfully",
                    NextSteps = "Login with new credentials"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Status = "Error",
                    ErrorCode = "InvalidToken",
                    Details = ex.Message
                });
            }
        }
    }
}