using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Bookstore.Data.Entities
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string BookName { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal DiscountPrice { get; set; }
        public int Quantity { get; set; }
        public string? BookImage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Link to Admin who added the book
        public int AdminId { get; set; }
        public Admin? Admin { get; set; }

        // Navigation properties
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    }
}