using System; // Basic .NET functionalities ko use karne ke liye
using System.Collections.Generic; // Collection types like List ko use karne ke liye
using System.Threading.Tasks; // Asynchronous programming ke liye
using Bookstore.Data.Entities; // Admin entity ko use karne ke liye
using Bookstore.Data.Interfaces; // Interface ko use karne ke liye
using Microsoft.EntityFrameworkCore; // Entity Framework Core ka use for DB operations

namespace Bookstore.Data.Repositories
{
    // AdminRepository class jo IAdminRepository interface ko implement kar rahi hai
    public class AdminRepository : IAdminRepository
    {
        private readonly AppDbContext _context; // DbContext ka reference, jisse DB se interact kiya jayega

        // Constructor jo context ko initialize karta hai
        public AdminRepository(AppDbContext context) => _context = context;

        // Admin ko register karne ka method
        public async Task<Admin> RegisterAdmin(Admin admin)
        {
            _context.Admins.Add(admin); // Admin ko Add karte hain database mein
            try
            {
                await _context.SaveChangesAsync(); // Database changes ko save karte hain
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx) // Agar error aaye to
            {
                // Inner exception ko throw karte hain, taaki controller handle kar sake
                throw new Exception(dbEx.InnerException?.Message, dbEx);
            }
            return admin; // Return karte hain newly added admin
        }

        // Admin ke login ke liye method (email aur password verify karna)
        public async Task<Admin> LoginAdmin(string email, string password)
        {
            // Database se admin ko email ke basis par search karte hain
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Email == email);
            if (admin == null || !BCrypt.Net.BCrypt.Verify(password, admin.Password)) // Agar admin nahi mila ya password match nahi kiya
                return null; // Toh null return karte hain
            return admin; // Agar credentials match karte hain toh admin ko return karte hain
        }

        // Admin ke email ko check karna ki wo exist karta hai ya nahi
        public async Task<bool> AdminExists(string email) =>
            await _context.Admins.AnyAsync(a => a.Email == email); // Agar email se koi admin milta hai, toh true return karenge

        // Email ke basis par admin ko fetch karna (auth verification ke liye)
        public async Task<Admin> GetByEmail(string email)
        {
            // Agar admin milta hai toh return karenge, nahi toh exception throw karenge
            return await _context.Admins.FirstOrDefaultAsync(a => a.Email == email)
                   ?? throw new InvalidOperationException("Admin not found");
        }

        // Sare admins ko asynchronously fetch karna
        public async Task<IEnumerable<Admin>> GetAllAdminsAsync()
        {
            return await _context.Admins.ToListAsync(); // List of all admins return karte hain
        }

        // Id ke basis par admin ko fetch karna
        public async Task<Admin?> GetAdminByIdAsync(int id)
        {
            return await _context.Admins.FindAsync(id); // Admin ko id se search karte hain
        }

        // Admin ko update karne ka method
        public async Task UpdateAdminAsync(Admin admin)
        {
            _context.Admins.Update(admin); // Admin ko update karte hain
            await _context.SaveChangesAsync(); // Changes ko save karte hain
        }

        // Admin ko delete karne ka method
        public async Task DeleteAdminAsync(int id)
        {
            var admin = await _context.Admins.FindAsync(id); // Admin ko id ke basis par search karte hain
            if (admin != null) // Agar admin milta hai toh
            {
                _context.Admins.Remove(admin); // Admin ko remove karte hain database se
                await _context.SaveChangesAsync(); // Changes ko save karte hain
            }
        }
    }
}
