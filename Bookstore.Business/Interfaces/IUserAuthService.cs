using System.Threading.Tasks;
using Bookstore.Data.Entities;

namespace Bookstore.Business.Interfaces
{
    public interface IUserAuthService
    {
        Task<User> Register(User user, string password);
        Task<User> Login(string email, string password); // Fixed return type
    }
}