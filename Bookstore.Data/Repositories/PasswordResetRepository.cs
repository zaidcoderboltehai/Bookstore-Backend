using System;
using System.Threading.Tasks;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Data.Repositories
{
    public class PasswordResetRepository : IPasswordResetRepository
    {
        private readonly AppDbContext _ctx;
        public PasswordResetRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task CreateAsync(PasswordReset reset)
        {
            _ctx.PasswordResets.Add(reset);
            await _ctx.SaveChangesAsync();
        }

        public Task<PasswordReset?> GetByTokenAsync(Guid token) =>
            _ctx.PasswordResets.FirstOrDefaultAsync(r => r.Token == token);

        public async Task DeleteAsync(PasswordReset reset)
        {
            _ctx.PasswordResets.Remove(reset);
            await _ctx.SaveChangesAsync();
        }
    }
}
