using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    public class UserRegisterDto
    {
        // 👇 First name zaroori hai, 2 se 50 characters ke beech hona chahiye, sirf alphabets allowed
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Must be 2-50 characters")]
        [RegularExpression(@"^[a-zA-Z]+$",
            ErrorMessage = "Only alphabets allowed")]
        public string FirstName { get; set; }

        // 👇 Last name bhi zaroori hai, same validation as first name
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Must be 2-50 characters")]
        [RegularExpression(@"^[a-zA-Z]+$",
            ErrorMessage = "Only alphabets allowed")]
        public string LastName { get; set; }

        // 👇 Email zaroori hai, aur format sahi hona chahiye (example: user@example.com)
        [Required(ErrorMessage = "Valid email required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        // 👇 Password strong hona chahiye: 8+ characters, ek uppercase, ek lowercase, aur ek number hona chahiye
        [Required(ErrorMessage = "Strong password required")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$",
            ErrorMessage = "8+ chars with uppercase, lowercase & number")]
        public string Password { get; set; }

        // 👇 Role optional hai, default value "User" hai, aur max 20 characters tak allowed hai
        [StringLength(20, ErrorMessage = "Role cannot exceed 20 characters")]
        public string? Role { get; set; } = "USER";  // Default value
    }

    public class UserLoginDto
    {
        // 👇 Login ke liye email zaroori hai, format bhi sahi hona chahiye
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Valid email format required. Example: user@example.com")]
        public string Email { get; set; }

        // 👇 Login ke liye password bhi zaroori hai
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
