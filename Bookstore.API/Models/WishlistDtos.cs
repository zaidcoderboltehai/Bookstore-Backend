using System;

namespace Bookstore.API.Models
{
    public class WishlistResponseDto
    {
        public int WishlistId { get; set; }
        public int BookId { get; set; }
        public string BookName { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public DateTime AddedAt { get; set; }
        public UserInfoDto User { get; set; }
    }

    public class UserInfoDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
    }
}