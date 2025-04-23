// Bookstore.Data/Entities/Cart.cs
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Data.Entities
{
    public class Cart
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int BookId { get; set; }

        [Required]
        public int Quantity { get; set; } = 1;

        [Required]
        public decimal PricePerUnit { get; set; }

        [Required]
        public bool IsPurchased { get; set; } = false;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User User { get; set; }
        public Book Book { get; set; }
    }
}