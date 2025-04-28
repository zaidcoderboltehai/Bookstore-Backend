using Bookstore.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Business.Interfaces
{
    public interface ICustomerAddressService
    {
        Task<CustomerAddress> AddAddressAsync(int userId, CustomerAddress address);
        Task<IEnumerable<CustomerAddress>> GetUserAddressesAsync(int userId);
        Task<CustomerAddress> GetAddressByIdAsync(int id);
        Task UpdateAddressAsync(CustomerAddress address);
        Task DeleteAddressAsync(int id);
    }
}