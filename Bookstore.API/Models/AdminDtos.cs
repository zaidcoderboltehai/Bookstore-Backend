using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    // Admin register karte waqt jo data chahiye, uske liye ye class banayi gayi hai
    public class AdminRegisterDto
    {
        // FirstName required hai aur sirf letters hone chahiye
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "First name must be between 2-50 characters")]
        [RegularExpression(@"^[A-Za-z]+$",
            ErrorMessage = "Only alphabetic characters allowed")]
        public string FirstName { get; set; }

        // LastName bhi required hai aur sirf letters hone chahiye
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Last name must be between 2-50 characters")]
        [RegularExpression(@"^[A-Za-z]+$",
            ErrorMessage = "Only alphabetic characters allowed")]
        public string LastName { get; set; }

        // Email required hai aur sahi format mein honi chahiye
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Must be a valid email format (e.g., user@example.com)")]
        public string Email { get; set; }

        // Password required hai aur usmein ek uppercase, ek lowercase aur ek number hona chahiye + length 8 ya usse zyada honi chahiye
        [Required(ErrorMessage = "Password is required")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must contain: 8+ characters, 1 uppercase, 1 lowercase, and 1 number")]
        public string Password { get; set; }

        // SecretKey required hai, jo sirf valid admins ke paas hota hai (security ke liye)
        [Required(ErrorMessage = "Valid admin secret key is required")]
        public string SecretKey { get; set; }
    }

    // Admin jab login karega, tab ye class use hogi
    public class AdminLoginDto
    {
        // Login ke liye email chahiye, sahi format mein
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        // Login ke liye password bhi chahiye
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
