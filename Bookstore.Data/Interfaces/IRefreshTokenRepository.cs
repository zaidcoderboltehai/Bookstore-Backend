using Bookstore.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Data.Interfaces
{
    public interface IRefreshTokenRepository
    {
        // Naya refresh token create karna
        Task<RefreshToken> CreateAsync(RefreshToken token);

        // Token ke basis pe refresh token dhoondhna (optional return type, agar nahi mila toh null)
        Task<RefreshToken?> FindByTokenAsync(string token);

        // Refresh token ko delete karna (id ke through)
        Task DeleteAsync(int id);

        // ✅ User-specific tokens ko cleanup karne ke liye method add kiya gaya hai
        Task<IEnumerable<RefreshToken>> GetByUserIdAsync(int userId);
    }
}
