using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bookstore.Data.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly AppDbContext _context;

        public AdminRepository(AppDbContext context) => _context = context;

        // Existing Methods
        public async Task<Admin> RegisterAdmin(Admin admin)
        {
            _context.Admins.Add(admin);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                throw new Exception(dbEx.InnerException?.Message, dbEx);
            }
            return admin;
        }

        public async Task<Admin> LoginAdmin(string email, string password)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
            return (admin != null && BCrypt.Net.BCrypt.Verify(password, admin.Password)) ? admin : null;
        }

        public async Task<bool> AdminExists(string email) =>
            await _context.Admins.AnyAsync(a => a.Email == email);

        public async Task<Admin> GetByEmail(string email) =>
            await _context.Admins.FirstOrDefaultAsync(a => a.Email == email)
            ?? throw new InvalidOperationException("Admin not found");

        // ✅ Added GetByExternalId Method
        public async Task<Admin?> GetByExternalId(string externalId) =>
            await _context.Admins.FirstOrDefaultAsync(a => a.ExternalId == externalId);

        public async Task<IEnumerable<Admin>> GetAllAdminsAsync() =>
            await _context.Admins.ToListAsync();

        public async Task<Admin?> GetAdminByIdAsync(int id) =>
            await _context.Admins.FindAsync(id);

        public async Task UpdateAdminAsync(Admin admin)
        {
            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAdminAsync(int id)
        {
            var admin = await _context.Admins.FindAsync(id);
            if (admin != null)
            {
                _context.Admins.Remove(admin);
                await _context.SaveChangesAsync();
            }
        }
    }
}