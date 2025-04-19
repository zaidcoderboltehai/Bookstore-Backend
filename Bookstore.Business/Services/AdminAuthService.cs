using Bookstore.Business.Interfaces; // IAdminAuthService ko use karna ke liye
using Bookstore.Data.Entities; // Admin entity ko use karne ke liye
using Bookstore.Data.Interfaces; // IAdminRepository ko use karne ke liye
using Microsoft.Extensions.Configuration; // Configuration settings ko read karne ke liye
using System; // Basic system functionalities ke liye
using System.Threading.Tasks; // Asynchronous programming ke liye, Task use hota hai

namespace Bookstore.Business.Services
{
    // ✅ Admin authentication se related service jo IAdminAuthService ko implement karti hai
    public class AdminAuthService : IAdminAuthService
    {
        private readonly IAdminRepository _adminRepo; // Admin repository ke liye reference
        private readonly string _validSecretKey; // Secret key jo configuration se milegi

        // Constructor: Jab AdminAuthService create hota hai, toh yeh dependency injection ke through adminRepo aur config liye jaate hain
        public AdminAuthService(
            IAdminRepository adminRepo, // AdminRepository ka reference
            IConfiguration config) // Configuration se data lene ke liye
        {
            _adminRepo = adminRepo; // _adminRepo ko initialize kar rahe hain
            _validSecretKey = config["AdminSecretKey"]; // appsettings.json se AdminSecretKey ki value le rahe hain
        }

        // Admin ko register karne ki method
        public async Task<Admin> Register(Admin admin, string password, string secretKey)
        {
            // 1. Secret key ko validate kar rahe hain
            if (secretKey != _validSecretKey) // Agar secret key match nahi karti, toh exception throw hoga
                throw new UnauthorizedAccessException("Invalid secret key");

            // 2. Check karte hain agar admin already exist karta hai ya nahi
            if (await _adminRepo.AdminExists(admin.Email)) // Agar admin ka email already exists, toh exception throw hoga
                throw new InvalidOperationException("Admin already exists");

            // 3. Password ko hash kar rahe hain, jisse secure storage ho sake
            admin.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(password, workFactor: 12); // BCrypt se password hash kar rahe hain

            // 4. Admin ko register kar rahe hain
            return await _adminRepo.RegisterAdmin(admin); // Admin repository se admin ko register kar rahe hain
        }

        // Admin login ki method
        public async Task<Admin> Login(string email, string password)
        {
            // 1. Admin ko email ke through retrieve kar rahe hain
            var admin = await _adminRepo.GetByEmail(email); // Admin ko email ke saath fetch karte hain

            // 2. Agar admin nahi mila ya password verify nahi hota, toh error throw hoga
            if (admin == null || !BCrypt.Net.BCrypt.EnhancedVerify(password, admin.Password)) // BCrypt se password verify kar rahe hain
                throw new UnauthorizedAccessException("Invalid email or password");

            return admin; // Agar sab kuch sahi hai, toh admin ko return kar rahe hain
        }
    }
}
