using Bookstore.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Data.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshToken> CreateAsync(RefreshToken token);
        Task<RefreshToken?> FindByTokenAsync(string token);
        Task DeleteAsync(int id);

        // ✅ Added method for user-specific token cleanup
        Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId);
    }
}