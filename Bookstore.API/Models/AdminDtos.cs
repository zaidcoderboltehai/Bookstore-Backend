using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    public class AdminRegisterDto
    {
        [Required(ErrorMessage = "First name is mandatory")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be 2-50 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is mandatory")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be 2-50 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must contain: 1 uppercase, 1 lowercase, 1 number, and be 8+ characters")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Valid secret key required for admin registration")]
        public string SecretKey { get; set; }
    }

    public class AdminLoginDto
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}