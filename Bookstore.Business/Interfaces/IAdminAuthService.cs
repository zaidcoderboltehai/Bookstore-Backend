using System.Threading.Tasks; // Asynchronous programming ke liye (Task<T> use hota hai)
using Bookstore.Data.Entities; // Admin entity ka reference liya (jo DB model hoga)

namespace Bookstore.Business.Interfaces
{
    // ✅ Ye interface define karta hai Admin ke authentication-related functions
    public interface IAdminAuthService
    {
        // 🔐 Register method ka contract:
        // Admin ko register karega with password aur ek secret key (jaise token banane ke liye ya authorization ke liye)
        // Return karega ek Admin object asynchronously (Task<Admin>)
        Task<Admin> Register(Admin admin, string password, string secretKey);

        // 🔑 Login method ka contract:
        // Email aur password lega input mein aur agar match ho gaya toh Admin object return karega
        // Return type Admin hai (matlab login ke baad Admin ki detail milegi), string ya token yahan nahi milta
        Task<Admin> Login(string email, string password);
    }
}
