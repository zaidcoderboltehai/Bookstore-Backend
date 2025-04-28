using Bookstore.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Data.Interfaces
{
    /// <summary>
    /// Interface for customer address data operations
    /// </summary>
    public interface ICustomerAddressRepository
    {
        /// <summary>
        /// Add a new customer address
        /// </summary>
        Task<CustomerAddress> AddAsync(CustomerAddress address);

        /// <summary>
        /// Get address by ID
        /// </summary>
        Task<CustomerAddress> GetByIdAsync(int id);

        /// <summary>
        /// Get all addresses for a user
        /// </summary>
        Task<IEnumerable<CustomerAddress>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Update an existing address
        /// </summary>
        Task UpdateAsync(CustomerAddress address);

        /// <summary>
        /// Delete an address
        /// </summary>
        Task DeleteAsync(CustomerAddress address);
    }
}