using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "First name must be 2-50 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Last name must be 2-50 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format. Example: user@example.com")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Must contain: uppercase, lowercase letters, and a number")]
        public string Password { get; set; }

        [StringLength(20, ErrorMessage = "Role cannot exceed 20 characters")]
        public string? Role { get; set; } = "User";  // Default value
    }

    public class UserLoginDto
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Valid email format required. Example: user@example.com")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}