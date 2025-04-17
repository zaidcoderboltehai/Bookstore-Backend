using System;
using System.Threading.Tasks;
using Bookstore.Data.Entities;

namespace Bookstore.Data.Interfaces
{
    public interface IPasswordResetRepository
    {
        Task CreateAsync(PasswordReset reset);
        Task<PasswordReset?> GetByTokenAsync(Guid token);
        Task DeleteAsync(PasswordReset reset);
    }
}
