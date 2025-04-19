using System; // System library ko include kiya gaya hai, jo basic data types aur utilities provide karta hai
using System.Threading.Tasks; // Asynchronous programming ke liye, Task use hota hai jo asynchronous operations ko handle karta hai

namespace Bookstore.Business.Interfaces
{
    // ✅ Yeh interface define karta hai ki password reset karne se related functionalities kya honi chahiye
    public interface IForgotPasswordService
    {
        // 📨 User ke liye password reset link bhejta hai, jo email ke through kiya jayega
        // Is function mein Task return hota hai, jo indicate karta hai ki yeh asynchronous (delay ho sakta hai) operation hai
        Task SendUserForgotPasswordLink(string email);

        // 📨 Admin ke liye password reset link bhejta hai, ismein ek extra parameter hai secretKey, jo admin verification ke liye use hota hai
        Task SendAdminForgotPasswordLink(string email, string secretKey);

        // 🔄 Password reset karta hai, ismein ek token aur new password diya jata hai 
        // Token verify karta hai ki yeh reset request valid hai, aur naya password set karte hai
        Task ResetPassword(Guid token, string newPassword);
    }
}
