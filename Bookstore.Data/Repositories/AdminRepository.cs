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

        public async Task<Admin> RegisterAdmin(Admin admin)
        {
            _context.Admins.Add(admin);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
            {
                // throw inner for controller to catch
                throw new Exception(dbEx.InnerException?.Message, dbEx);
            }
            return admin;
        }

        public async Task<Admin> LoginAdmin(string email, string password)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
            if (admin == null || !BCrypt.Net.BCrypt.Verify(password, admin.Password))
                return null;
            return admin;
        }

        public async Task<bool> AdminExists(string email) =>
            await _context.Admins.AnyAsync(a => a.Email == email);

        public async Task<Admin> GetByEmail(string email)
        {
            return await _context.Admins.FirstOrDefaultAsync(a => a.Email == email)
                   ?? throw new InvalidOperationException("Admin not found");
        }

        public async Task<IEnumerable<Admin>> GetAllAdminsAsync()
        {
            return await _context.Admins.ToListAsync();
        }

        public async Task<Admin?> GetAdminByIdAsync(int id)
        {
            return await _context.Admins.FindAsync(id);
        }

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
