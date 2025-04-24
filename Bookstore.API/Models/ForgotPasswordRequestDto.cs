using System;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    // 👇 Ye class tab use hoti hai jab koi user ya admin apna password bhool jaata hai
    public class ForgotPasswordRequestDto
    {
        // 👇 Email address dena zaroori hai (aur check bhi hoga ki format sahi hai ya nahi)
        [Required, EmailAddress]
        public string Email { get; set; }

        // 👇 Ye optional property hai — sirf admin ke liye hoti hai verify karne ke liye
        public string? SecretKey { get; set; } // Optional, sirf admin ke liye
    }

    // 👇 Ye class tab kaam aati hai jab user ya admin apna password reset karta hai
    public class ResetPasswordDto
    {
        // 👇 Reset token dena zaroori hai (ye token forgot password ke response mein milta hai)
        [Required]
        public Guid Token { get; set; }

        // 👇 Naya password set karne ke liye (minimum 6 characters hona chahiye)
        [Required, MinLength(6)]
        public string NewPassword { get; set; }
    }
}
