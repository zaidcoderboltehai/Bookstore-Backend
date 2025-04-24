using System;
using System.Threading.Tasks;
using Bookstore.Data.Entities;

namespace Bookstore.Data.Interfaces
{
    public interface IPasswordResetRepository
    {
        // Password reset ke liye naya record create karna
        Task CreateAsync(PasswordReset reset);

        // Token ke basis pe password reset record fetch karna (optional return type, agar nahi mila toh null)
        Task<PasswordReset?> GetByTokenAsync(Guid token);

        // Password reset record ko delete karna
        Task DeleteAsync(PasswordReset reset);
    }
}
