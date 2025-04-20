using System.Security.Claims;

namespace Bookstore.Business.Interfaces
{
    /// <summary>
    /// Service contract for JWT token generation and validation operations
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT access token with user claims
        /// </summary>
        /// <param name="entity">User/Admin entity to generate token for</param>
        /// <returns>Signed JWT token string</returns>
        // 🏷️ Yeh method JWT access token generate karta hai user ya admin ke claims ke saath
        // Ismein dynamic entity pass hota hai jo user ya admin ki details ko represent karta hai
        string CreateToken(dynamic entity);

        /// <summary>
        /// Generates a cryptographically secure refresh token
        /// </summary>
        /// <returns>Base64 encoded refresh token</returns>
        // 🔄 Yeh method cryptographically secure refresh token generate karta hai
        // Refresh token ko Base64 mein encode karke return karta hai
        string GenerateRefreshToken();

        /// <summary>
        /// Extracts claims principal from expired access token
        /// </summary>
        /// <param name="token">Expired JWT access token</param>
        /// <returns>Validated claims principal</returns>
        /// <exception cref="SecurityTokenException">Thrown for invalid/expired tokens</exception>
        // 🕵️‍♂️ Yeh method expired JWT access token se claims principal extract karta hai
        // Agar token invalid ya expired ho toh SecurityTokenException throw hota hai
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
