using System; // System namespace ko import kar rahe hain jo basic functionality ke liye zaroori hai
using System.ComponentModel.DataAnnotations; // Data validation ke liye zaroori namespace ko import kiya gaya hai

namespace Bookstore.Data.Entities
{
    // PasswordReset class jo password reset ke liye details ko store karegi
    public class PasswordReset
    {
        [Key] // Yeh annotation 'Token' ko primary key banaata hai, jisme unique identifier hoga
        public Guid Token { get; set; } // Token jo unique hota hai aur password reset link ko identify karega

        [Required] // Yeh annotation 'Email' ko required banata hai, matlab isse empty nahi chhoda jaa sakta
        public string Email { get; set; } = string.Empty; // Email address jise password reset request kiya gaya hai

        [Required] // Yeh annotation 'ExpiryUtc' ko required banata hai, matlab expiry time ko empty nahi chhoda jaa sakta
        public DateTime ExpiryUtc { get; set; } // Expiry time jisme token expire ho jaayega, UTC time mein store hoga
    }
}
