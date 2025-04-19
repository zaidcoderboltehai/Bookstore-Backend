using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    public class UserUpdateDto
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
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
    }
}