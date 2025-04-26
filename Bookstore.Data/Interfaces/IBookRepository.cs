using Bookstore.Data.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bookstore.Data.Interfaces
{
    public interface IBookRepository
    {
        // ✅ CRUD Operations
        Task<Book> AddAsync(Book book);
        Task AddRangeAsync(IEnumerable<Book> books); // 🆕 Batch add for CSV import
        Task<IEnumerable<Book>> GetAllAsync();
        Task<Book?> GetByIdAsync(int id);
        Task UpdateAsync(Book book);
        Task DeleteAsync(int id);

        // ✅ Search/Sort/Utilities
        Task<IEnumerable<Book>> SearchByAuthorAsync(string author);
        Task<IEnumerable<Book>> SortByPriceAsync(bool ascending = true);
        Task<IEnumerable<Book>> GetRecentAsync(int count);
    }
}