using System.ComponentModel.DataAnnotations; // Validation ke liye necessary namespace ko import kar rahe hain

namespace Bookstore.Data.Entities
{
    // Admin class jo ki ek entity ko represent karti hai database ke liye
    public class Admin
    {
        [Key] // Yeh annotation bata raha hai ki 'Id' property ko primary key banaana hai database mein
        public int Id { get; set; } // Admin ka unique ID, jise primary key ke roop mein use kiya jaayega

        [Required] // Yeh annotation bata raha hai ki 'FirstName' field required hai, empty nahi ho sakti
        public string FirstName { get; set; } = string.Empty; // Default value empty string ke saath initialize kiya gaya hai

        [Required] // Yeh annotation bata raha hai ki 'LastName' field bhi required hai
        public string LastName { get; set; } = string.Empty; // Default value empty string ke saath initialize kiya gaya hai

        [Required] // Yeh annotation bata raha hai ki 'Email' field required hai
        [EmailAddress] // Yeh annotation validate karega ki jo email diya gaya hai, woh ek valid email format mein ho
        public string Email { get; set; } = string.Empty; // Default value empty string ke saath initialize kiya gaya hai

        [Required] // Yeh annotation bata raha hai ki 'Password' field required hai
        public string Password { get; set; } = string.Empty; // Default value empty string ke saath initialize kiya gaya hai

        [Required] // Yeh annotation bata raha hai ki 'SecretKey' field required hai
        public string SecretKey { get; set; } = string.Empty; // Default value empty string ke saath initialize kiya gaya hai

        [Required] // Yeh annotation bata raha hai ki 'Role' field required hai
        public string Role { get; set; } = "Admin"; // Default value "Admin" hai, jo bata raha hai ki by default yeh admin hi hoga
    }
}
