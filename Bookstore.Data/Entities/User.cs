using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Data.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = "User";

        // Navigation properties
        public ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();

        // New navigation properties
        public ICollection<CustomerAddress> CustomerAddresses { get; set; } = new List<CustomerAddress>();
        public ICollection<OrderSummary> Orders { get; set; } = new List<OrderSummary>();
    }
}