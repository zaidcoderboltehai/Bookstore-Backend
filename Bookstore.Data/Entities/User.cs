using System.ComponentModel.DataAnnotations;

namespace Bookstore.Data.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty; // Initialize with empty string

        [Required]
        public string LastName { get; set; } = string.Empty; // Initialize with empty string

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty; // Initialize with empty string

        [Required]
        public string Password { get; set; } = string.Empty; // Initialize with empty string

        [Required]
        public string Role { get; set; } = "User";
    }
}
