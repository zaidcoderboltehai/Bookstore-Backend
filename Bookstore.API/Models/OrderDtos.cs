using System.ComponentModel.DataAnnotations;

namespace Bookstore.API.Models
{
    // 👇 Ye DTO order create karte waqt address select karne ke liye use hoga
    public class CreateOrderDto
    {
        [Required(ErrorMessage = "Address ID is required")]
        public int AddressId { get; set; }
    }

    // Optional: Agar order response ke liye alag DTO chahiye toh
    public class OrderResponseDto
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        // ... aur properties
    }
}