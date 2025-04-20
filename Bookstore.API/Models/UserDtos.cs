using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    public class UserRegisterDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Must be 2-50 characters")]
        [RegularExpression(@"^[a-zA-Z]+$",
            ErrorMessage = "Only alphabets allowed")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Must be 2-50 characters")]
        [RegularExpression(@"^[a-zA-Z]+$",
            ErrorMessage = "Only alphabets allowed")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Valid email required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Strong password required")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$",
            ErrorMessage = "8+ chars with uppercase, lowercase & number")]
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