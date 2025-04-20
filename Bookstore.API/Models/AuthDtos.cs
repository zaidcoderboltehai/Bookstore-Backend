using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    // Jab user ka access token expire ho jaye, toh naye token ke liye ye request bheji jaati hai
    public class RefreshTokenRequest
    {
        // AccessToken dena zaroori hai
        [Required(ErrorMessage = "Access token required")]
        public string AccessToken { get; set; }

        // RefreshToken bhi dena zaroori hai
        [Required(ErrorMessage = "Refresh token required")]
        public string RefreshToken { get; set; }
    }

    // Ye class tab use hoti hai jab hume check karna ho ki koi token valid hai ya nahi
    public class TokenValidationRequest
    {
        // Sirf token pass karna hota hai, validation backend karega
        public string Token { get; set; }
    }
}
