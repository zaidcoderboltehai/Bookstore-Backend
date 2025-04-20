using System.Collections.Generic; // Collection types ko handle karne ke liye
using System.Threading.Tasks; // Asynchronous programming ko support karne ke liye
using Bookstore.Data.Entities; // User entity ko use karne ke liye
using Bookstore.Data.Interfaces; // Interface ko use karne ke liye
using Microsoft.EntityFrameworkCore; // Entity Framework Core ka use for DB operations
using BCrypt.Net; // ✅ Namespace added for BCrypt, jo password encryption ke liye use hota hai

namespace Bookstore.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context; // DbContext ka reference, jo DB se interact karta hai

        // Constructor jo context ko initialize karta hai
        public UserRepository(AppDbContext context) => _context = context;

        // User Registration ka method
        public async Task<User> RegisterUser(User user)
        {
            _context.Users.Add(user); // User ko DB mein add karte hain
            await _context.SaveChangesAsync(); // Changes ko DB mein save karte hain
            return user; // Registered user ko return karte hain
        }

        // Authentication (BCrypt EnhancedVerify Fix ke saath password verify karna)
        public async Task<User?> LoginUser(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email); // Email ke basis par user ko search karte hain
            return (user != null && BCrypt.Net.BCrypt.EnhancedVerify(password, user.Password)) // Agar user milta hai aur password match karta hai
                ? user // Toh user ko return karte hain
                : null; // Agar match nahi hota, toh null return karte hain
        }

        // Check karna ki user already exist karta hai ya nahi
        public async Task<bool> UserExists(string email) =>
            await _context.Users.AnyAsync(u => u.Email == email); // Agar email se user milta hai, toh true return karte hain

        // Sare users ko asynchronously fetch karna
        public async Task<IEnumerable<User>> GetAllUsersAsync() =>
            await _context.Users.ToListAsync(); // All users ko list mein convert karke return karte hain

        // Id ke basis par user ko fetch karna
        public async Task<User?> GetUserByIdAsync(int id) =>
            await _context.Users.FindAsync(id); // User ko id se search karte hain

        // Email ke basis par user ko fetch karna
        public async Task<User?> GetUserByEmail(string email) =>
            await _context.Users.FirstOrDefaultAsync(u => u.Email == email); // Email ke basis par user ko search karte hain

        // User ko update karne ka method
        public async Task UpdateUserAsync(User user)
        {
            _context.Entry(user).State = EntityState.Modified; // Entity ko modify state mein mark karte hain
            await _context.SaveChangesAsync(); // Changes ko DB mein save karte hain
        }

        // User ko delete karne ka method
        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id); // User ko id ke basis par search karte hain
            if (user != null) // Agar user milta hai
            {
                _context.Users.Remove(user); // User ko DB se remove karte hain
                await _context.SaveChangesAsync(); // Changes ko DB mein save karte hain
            }
        }
    }
}
