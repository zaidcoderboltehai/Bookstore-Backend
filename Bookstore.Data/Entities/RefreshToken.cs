using System.ComponentModel.DataAnnotations;

namespace Bookstore.Data.Entities
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public int UserId { get; set; }
        public string UserType { get; set; } // "Admin" or "User"
    }
}