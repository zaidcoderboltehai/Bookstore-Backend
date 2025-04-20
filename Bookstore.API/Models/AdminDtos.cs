using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    public class AdminRegisterDto
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "First name must be between 2-50 characters")]
        [RegularExpression(@"^[A-Za-z]+$",
            ErrorMessage = "Only alphabetic characters allowed")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Last name must be between 2-50 characters")]
        [RegularExpression(@"^[A-Za-z]+$",
            ErrorMessage = "Only alphabetic characters allowed")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Must be a valid email format (e.g., user@example.com)")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must contain: 8+ characters, 1 uppercase, 1 lowercase, and 1 number")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Valid admin secret key is required")]
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