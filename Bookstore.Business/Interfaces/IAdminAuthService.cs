using System.Threading.Tasks;
using Bookstore.Data.Entities;

namespace Bookstore.Business.Interfaces
{
    public interface IAdminAuthService
    {
        Task<Admin> Register(Admin admin, string password, string secretKey);
        Task<Admin> Login(string email, string password); // Return type Admin (not string)
    }
}