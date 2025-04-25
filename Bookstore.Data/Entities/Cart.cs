using System;
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
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        [Required]
        [DataType(DataType.Currency)]
        public decimal PricePerUnit { get; set; }

        [Required]
        public bool IsPurchased { get; set; } = false;

        [Required]
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;

        // Nullable until purchased
        public DateTime? PurchasedAt { get; set; }

        // Navigation properties
        public User User { get; set; }
        public Book Book { get; set; }
    }
}
