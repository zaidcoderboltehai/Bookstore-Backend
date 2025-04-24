using System.ComponentModel.DataAnnotations; // Data annotations ke liye necessary namespace import kiya gaya hai

namespace Bookstore.Data.Entities
{
    public class RefreshToken
    {
        [Key] // Yeh annotation 'Id' property ko primary key banaata hai
        public int Id { get; set; } // Refresh token ka unique identifier (primary key)

        public string Token { get; set; } // Refresh token jo user ke liye generate hota hai

        public DateTime Expires { get; set; } // Yeh property token ki expiry date ko store karti hai

        public bool IsExpired => DateTime.UtcNow >= Expires; // Yeh property check karti hai ki token expire ho gaya hai ya nahi

        public int UserId { get; set; } // Yeh field user ka unique identifier (ID) store karta hai

        public string UserType { get; set; } // User ka type store karta hai (Admin ya User)
    }
}
