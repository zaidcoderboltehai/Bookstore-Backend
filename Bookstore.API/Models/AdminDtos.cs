namespace Bookstore.API.Models
{
    public class AdminRegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string SecretKey { get; set; } // Only for Admin Registration
    }

    public class AdminLoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}