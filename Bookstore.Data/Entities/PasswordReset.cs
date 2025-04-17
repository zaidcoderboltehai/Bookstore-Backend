using System;
using System.ComponentModel.DataAnnotations;

namespace Bookstore.Data.Entities
{
    public class PasswordReset
    {
        [Key]
        public Guid Token { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiryUtc { get; set; }
    }
}
