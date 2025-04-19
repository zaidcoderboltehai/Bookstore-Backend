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
        string CreateToken(dynamic entity);

        /// <summary>
        /// Generates a cryptographically secure refresh token
        /// </summary>
        /// <returns>Base64 encoded refresh token</returns>
        string GenerateRefreshToken();

        /// <summary>
        /// Extracts claims principal from expired access token
        /// </summary>
        /// <param name="token">Expired JWT access token</param>
        /// <returns>Validated claims principal</returns>
        /// <exception cref="SecurityTokenException">Thrown for invalid/expired tokens</exception>
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}