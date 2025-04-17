using System;
using System.Threading.Tasks;

namespace Bookstore.Business.Interfaces
{
    public interface IForgotPasswordService
    {
        Task SendUserForgotPasswordLink(string email);
        Task SendAdminForgotPasswordLink(string email, string secretKey);
        Task ResetPassword(Guid token, string newPassword);
    }
}
