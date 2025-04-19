using System.ComponentModel.DataAnnotations; // Data validation ke liye necessary namespace

namespace Bookstore.Data.Entities
{
    // User class, jo user ke information ko store karega
    public class User
    {
        [Key] // Yeh annotation 'Id' ko primary key banaata hai, jo har user ko uniquely identify karega
        public int Id { get; set; } // User ka unique identifier (Primary Key)

        [Required] // Yeh annotation 'FirstName' ko required banaata hai, matlab yeh field empty nahi chhoda jaa sakta
        public string FirstName { get; set; } = string.Empty; // User ka first name. Default value empty string

        [Required] // Yeh annotation 'LastName' ko required banaata hai, matlab yeh field empty nahi chhoda jaa sakta
        public string LastName { get; set; } = string.Empty; // User ka last name. Default value empty string

        [Required] // Yeh annotation 'Email' ko required banaata hai aur usse email format mein hona chahiye
        [EmailAddress] // Email format ko validate karta hai (email address valid hona chahiye)
        public string Email { get; set; } = string.Empty; // User ka email address. Default value empty string

        [Required] // Yeh annotation 'Password' ko required banaata hai, matlab yeh field empty nahi chhoda jaa sakta
        public string Password { get; set; } = string.Empty; // User ka password. Default value empty string

        [Required] // Yeh annotation 'Role' ko required banaata hai, jo user ka role specify karega
        public string Role { get; set; } = "User"; // User ka role. Default value "User" set kiya gaya hai
    }
}
