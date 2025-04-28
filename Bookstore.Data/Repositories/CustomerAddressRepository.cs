using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class CustomerAddressRepository : ICustomerAddressRepository
    {
        private readonly AppDbContext _context;

        public CustomerAddressRepository(AppDbContext context) => _context = context;

        public async Task<CustomerAddress> AddAsync(CustomerAddress address)
        {
            _context.CustomerAddresses.Add(address);
            await _context.SaveChangesAsync();
            return address;
        }

        public async Task<CustomerAddress> GetByIdAsync(int id)
            => await _context.CustomerAddresses.FindAsync(id);

        public async Task<IEnumerable<CustomerAddress>> GetByUserIdAsync(int userId)
            => await _context.CustomerAddresses
                .Where(ca => ca.UserId == userId)
                .ToListAsync();

        public async Task UpdateAsync(CustomerAddress address)
        {
            _context.Entry(address).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(CustomerAddress address)
        {
            _context.CustomerAddresses.Remove(address);
            await _context.SaveChangesAsync();
        }
    }
}