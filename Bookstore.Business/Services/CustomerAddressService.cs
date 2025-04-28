using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Business.Services
{
    public class CustomerAddressService : ICustomerAddressService
    {
        private readonly ICustomerAddressRepository _repo;

        public CustomerAddressService(ICustomerAddressRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<CustomerAddress> AddAddressAsync(int userId, CustomerAddress address)
        {
            // Validate input
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));

            address.UserId = userId;
            address.CreatedAt = DateTime.UtcNow;
            address.UpdatedAt = DateTime.UtcNow;

            return await _repo.AddAsync(address);
        }

        public async Task<IEnumerable<CustomerAddress>> GetUserAddressesAsync(int userId)
        {
            if (userId <= 0) throw new ArgumentException("Invalid user ID", nameof(userId));
            return await _repo.GetByUserIdAsync(userId);
        }

        public async Task<CustomerAddress> GetAddressByIdAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid address ID", nameof(id));

            var address = await _repo.GetByIdAsync(id);
            return address ?? throw new KeyNotFoundException($"Address with ID {id} not found");
        }

        public async Task UpdateAddressAsync(CustomerAddress address)
        {
            if (address == null) throw new ArgumentNullException(nameof(address));
            if (address.Id <= 0) throw new ArgumentException("Invalid address ID", nameof(address.Id));

            address.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateAsync(address);
        }

        public async Task DeleteAddressAsync(int id)
        {
            if (id <= 0) throw new ArgumentException("Invalid address ID", nameof(id));

            var address = await _repo.GetByIdAsync(id);
            if (address == null) throw new KeyNotFoundException($"Address with ID {id} not found");

            await _repo.DeleteAsync(address);
        }
    }
}