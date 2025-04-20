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
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config; // Configuration se JWT ke settings lene ke liye
        private readonly SymmetricSecurityKey _signingKey; // JWT sign karne ke liye key

        // Constructor: Jab TokenService banayi jaati hai, toh configuration ko inject kiya jaata hai
        public TokenService(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config)); // Agar config null hai toh error throw karega
            _signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"])); // Key ko UTF-8 mein encode kar rahe hain
        }

        // Token create karne ki method
        public string CreateToken(dynamic entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null for token creation"); // Agar entity null ho, toh error throw karega

            if (entity?.Id == null)
                throw new ArgumentException("Entity must have a valid ID", nameof(entity.Id)); // Agar entity ka ID null ho, toh error throw karega

            var claims = new List<Claim> // Claims list banayi jaa rahi hai jo token mein jayegi
            {
                // Dual claim mapping for compatibility
                new Claim(JwtRegisteredClaimNames.Sub, entity.Id.ToString()), // Entity ka ID claim ke roop mein set kar rahe hain
                new Claim(ClaimTypes.NameIdentifier, entity.Id.ToString()), // NameIdentifier claim set kar rahe hain
                new Claim(JwtRegisteredClaimNames.Email, entity.Email), // Email claim set kar rahe hain
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT ID claim set kar rahe hain
                new Claim(ClaimTypes.Role, GetEntityRole(entity)), // Role claim set kar rahe hain
                new Claim("UserType", GetEntityRole(entity)), // UserType claim ko add kar rahe hain
                new Claim("FullName", $"{entity.FirstName} {entity.LastName}"), // Full name ko claim ke roop mein set kar rahe hain
                new Claim("EntityType", entity.GetType().Name), // Entity type ko claim ke roop mein set kar rahe hain
                new Claim("AuditStamp", DateTime.UtcNow.ToString("yyyyMMddHHmmss")) // Audit stamp ke liye date aur time set kar rahe hain
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims), // Claims ko identity ke roop mein set kar rahe hain
                Expires = DateTime.UtcNow.AddMinutes(30), // Token ka expiry time set kar rahe hain
                SigningCredentials = new SigningCredentials(
                    _signingKey, // Signing key ka use kar rahe hain
                    SecurityAlgorithms.HmacSha512Signature), // HMAC SHA-512 algorithm ka use kar rahe hain
                Issuer = _config["Jwt:Issuer"], // Issuer ko config se le rahe hain
                Audience = _config["Jwt:Audience"], // Audience ko config se le rahe hain
                NotBefore = DateTime.UtcNow // Token ko abhi se valid banane ke liye NotBefore set kar rahe hain
            };

            var tokenHandler = new JwtSecurityTokenHandler(); // Token handler create kar rahe hain
            var token = tokenHandler.CreateToken(tokenDescriptor); // Token ko create kar rahe hain

            return tokenHandler.WriteToken(token); // Token ko string mein convert kar ke return kar rahe hain
        }

        // Refresh token generate karne ki method
        public string GenerateRefreshToken()
        {
            using var rng = RandomNumberGenerator.Create(); // Random number generator ka use kar rahe hain
            var randomBytes = new byte[64]; // 64 bytes ka random array banaya hai
            rng.GetBytes(randomBytes); // Random bytes ko fill kar rahe hain
            return Convert.ToBase64String(randomBytes); // Random bytes ko base64 string mein convert kar ke return kar rahe hain
        }

        // Expired token se principal extract karne ki method
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler(); // Token handler create kar rahe hain

            try
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, // Issuer validation enable kar rahe hain
                    ValidIssuer = _config["Jwt:Issuer"], // Issuer ko config se le rahe hain
                    ValidateAudience = true, // Audience validation enable kar rahe hain
                    ValidAudience = _config["Jwt:Audience"], // Audience ko config se le rahe hain
                    ValidateIssuerSigningKey = true, // Issuer signing key validation enable kar rahe hain
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(_config["Jwt:Key"])), // Signing key ko config se le rahe hain
                    ValidateLifetime = false, // Lifetime validation disable kar rahe hain kyunki token expired ho chuka hai
                    ClockSkew = TimeSpan.Zero, // Clock skew ko zero set kar rahe hain
                    NameClaimType = JwtRegisteredClaimNames.Sub, // Name claim ko configure kar rahe hain
                    RoleClaimType = ClaimTypes.Role // Role claim ko configure kar rahe hain
                };

                return tokenHandler.ValidateToken(token, validationParameters, out _); // Token ko validate kar rahe hain aur principal ko return kar rahe hain
            }
            catch (SecurityTokenValidationException ex)
            {
                throw new SecurityTokenException("Invalid token structure", ex); // Agar token validation fail ho, toh error throw karega
            }
            catch (Exception ex)
            {
                throw new SecurityTokenException("Token validation failed", ex); // Agar koi aur error aaye, toh error throw karega
            }
        }

        // Entity ke role ko identify karne ki method
        private string GetEntityRole(dynamic entity)
        {
            return entity switch // Entity ke type ke according role return kar rahe hain
            {
                Admin => "Admin", // Agar entity Admin hai, toh "Admin" return karenge
                User => "User", // Agar entity User hai, toh "User" return karenge
                _ => throw new InvalidOperationException("Unsupported entity type") // Agar entity type support nahi karte, toh error throw karega
            };
        }
    }
}
