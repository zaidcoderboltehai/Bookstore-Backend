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
    /// Handles JWT token generation and validation
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly SymmetricSecurityKey _signingKey;

        public TokenService(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])
            );
        }

        /// <summary>
        /// Generates JWT access token from entity properties
        /// </summary>
        public string CreateToken(dynamic entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity required for token generation");

            var claims = new List<Claim>
            {
                // Core identity claims
                new Claim(ClaimTypes.NameIdentifier, entity.Id.ToString()),
                new Claim("role", entity.Role.ToUpper()), // ✅ Ensures uppercase role values
                new Claim(JwtRegisteredClaimNames.Email, entity.Email),
                
                // Additional metadata
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("FullName", $"{entity.FirstName} {entity.LastName}"),
                new Claim("AccountType", entity is User ? "StandardUser" : "Admin")
            };

            return CreateTokenFromClaims(claims);
        }

        /// <summary>
        /// Generates token from existing claims
        /// </summary>
        public string CreateTokenFromClaims(IEnumerable<Claim> claims)
        {
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    _signingKey,
                    SecurityAlgorithms.HmacSha512Signature
                ),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                NotBefore = DateTime.UtcNow
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        /// <summary>
        /// Generates cryptographically secure refresh token
        /// </summary>
        public string GenerateRefreshToken()
        {
            using var rng = RandomNumberGenerator.Create();
            var randomBytes = new byte[64];
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        /// <summary>
        /// Validates expired token and extracts principal
        /// </summary>
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            return tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _config["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _config["Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,
                ValidateLifetime = false, // Allow expired tokens
                ClockSkew = TimeSpan.Zero,

                // Critical claim type configuration
                NameClaimType = ClaimTypes.NameIdentifier,
                RoleClaimType = "role" // ✅ Matches claim type used in token creation
            }, out _);
        }
    }
}