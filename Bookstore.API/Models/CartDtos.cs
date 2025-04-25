using System;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    public class CartResponseDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookName { get; set; } = default!;
        public string Author { get; set; } = default!;
        public decimal PricePerUnit { get; set; }
        public int Quantity { get; set; }
        public DateTime AddedAt { get; set; }

        // Naye fields:
        public bool IsPurchased { get; set; }          // purchase status
        public DateTime? PurchasedAt { get; set; }     // kab purchase hua
    }

    public class CartRequestDto
    {
        [Required(ErrorMessage = "Book ID is required")]
        public int BookId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
    }
}
