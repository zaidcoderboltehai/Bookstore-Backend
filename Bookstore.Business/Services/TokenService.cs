using Bookstore.Data.Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Bookstore.Business.Interfaces;

namespace Bookstore.Business.Services
{
    /// <summary>
    /// Service to handle JWT token generation and validation
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _signingKey;

        public TokenService(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])); // Load JWT secret key
        }

        /// <summary>
        /// Generate JWT token from entity properties
        /// </summary>
        public string CreateToken(dynamic entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null to generate token");

            var claims = new List<Claim>
            {
                // ✅ Add user ID and role claims
                new Claim(ClaimTypes.NameIdentifier, entity.Id.ToString()),
                new Claim(ClaimTypes.Role, entity.Role), // "Admin" or "User"
                new Claim(JwtRegisteredClaimNames.Email, entity.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("FullName", $"{entity.FirstName} {entity.LastName}")
            };

            return CreateTokenFromClaims(claims);
        }

        /// <summary>
        /// Generate JWT token directly from claims
        /// </summary>
        public string CreateTokenFromClaims(IEnumerable<Claim> claims)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30), // Token validity: 30 minutes
                SigningCredentials = new SigningCredentials(
                    _signingKey,
                    SecurityAlgorithms.HmacSha512Signature), // Signature algorithm
                Issuer = _config["Jwt:Issuer"], // Token issuer
                Audience = _config["Jwt:Audience"], // Token target audience
                NotBefore = DateTime.UtcNow // Token valid from now
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token); // Convert token to string
        }

        /// <summary>
        /// Generate a secure refresh token
        /// </summary>
        public string GenerateRefreshToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var randomBytes = new byte[64];
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes); // Base64 encoded token
        }

        /// <summary>
        /// Extract claims from an expired token
        /// </summary>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"],
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_config["Jwt:Key"])), // Signing key
                    ValidateLifetime = false, // Allow validation of expired token
                    ClockSkew = TimeSpan.Zero, // No time sync relaxation
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = ClaimTypes.Role // ✅ Recognize role claim
                };

                return tokenHandler.ValidateToken(token, validationParameters, out _);
            }
            catch (SecurityTokenValidationException ex)
            {
                throw new SecurityTokenException("Invalid token structure", ex);
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Token validation failed", ex);
            }
        }
    }
}
