using System.ComponentModel.DataAnnotations;

namespace Bookstore.Data.Entities
{
    public class Wishlist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; }
        public Book Book { get; set; }
    }
}