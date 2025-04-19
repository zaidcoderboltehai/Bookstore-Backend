using System;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    // 👇 Ye class tab use hoti hai jab koi user ya admin password bhool jaata hai
    public class ForgotPasswordRequestDto
    {
        // 👇 Email address dena zaroori hai (validate bhi ho raha hai ki sahi format mein ho)
        [Required, EmailAddress]
        public string Email { get; set; }

        // 👇 Ye optional hai — sirf admin ke liye use hota hai password reset verify karne ke liye
        public string? SecretKey { get; set; } // Optional, for admin
    }

    // 👇 Ye class tab use hoti hai jab user ya admin password reset karta hai
    public class ResetPasswordDto
    {
        // 👇 Reset token required hai (ye token forgot-password ke response se milta hai)
        [Required]
        public Guid Token { get; set; }

        // 👇 Naya password jo user set karega (minimum 6 characters hona chahiye)
        [Required, MinLength(6)]
        public string NewPassword { get; set; }
    }
}
