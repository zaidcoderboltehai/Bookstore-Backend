using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    public class RefreshTokenRequest
    {
        [Required(ErrorMessage = "Access token required")]
        public string AccessToken { get; set; }

        [Required(ErrorMessage = "Refresh token required")]
        public string RefreshToken { get; set; }
    }

    // Optional: Agar aur authentication-related DTOs hain toh yahi add karo
    public class TokenValidationRequest
    {
        public string Token { get; set; }
    }
}