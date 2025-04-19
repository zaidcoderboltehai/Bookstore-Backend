using System.Collections.Generic;
using System.Threading.Tasks;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net; // ✅ Namespace added for BCrypt

namespace Bookstore.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) => _context = context;

        // User Registration
        public async Task<User> RegisterUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        // Authentication (BCrypt EnhancedVerify Fix)
        public async Task<User?> LoginUser(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return (user != null && BCrypt.Net.BCrypt.EnhancedVerify(password, user.Password))
                ? user
                : null;
        }

        public async Task<bool> UserExists(string email) =>
            await _context.Users.AnyAsync(u => u.Email == email);

        // User Management
        public async Task<IEnumerable<User>> GetAllUsersAsync() =>
            await _context.Users.ToListAsync();

        public async Task<User?> GetUserByIdAsync(int id) =>
            await _context.Users.FindAsync(id);

        public async Task<User?> GetUserByEmail(string email) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        // Update Operations
        public async Task UpdateUserAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        // Delete Operation
        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
    }
}