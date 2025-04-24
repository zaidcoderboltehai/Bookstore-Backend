using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    public class UserUpdateDto
    {
        // 👇 First name update karne ke liye zaroori hai, 2 se 50 characters ke beech hona chahiye
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "First name must be 2-50 characters")]
        public string FirstName { get; set; }

        // 👇 Last name bhi zaroori hai, 2 se 50 characters ke beech hona chahiye
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Last name must be 2-50 characters")]
        public string LastName { get; set; }

        // 👇 Email address update karte waqt bhi zaroori hai, aur sahi format mein hona chahiye
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
    }
}
