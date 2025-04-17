using System;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    public class ForgotPasswordRequestDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }

        public string? SecretKey { get; set; } // Optional, for admin
    }

    public class ResetPasswordDto
    {
        [Required]
        public Guid Token { get; set; }

        [Required, MinLength(6)]
        public string NewPassword { get; set; }
    }
}
