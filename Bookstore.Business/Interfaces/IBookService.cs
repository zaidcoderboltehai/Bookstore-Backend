using Bookstore.Data.Entities;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Bookstore.Business.Interfaces
{
    public interface IBookService
    {
        // Create
        Task<Book> AddBookAsync(Book book);

        // Bulk Import
        Task ImportBooksFromCsvAsync(Stream fileStream, int adminId);

        // Read
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book?> GetBookByIdAsync(int id);

        // Update/Delete
        Task UpdateBookAsync(Book book);
        Task DeleteBookAsync(int id);

        // Search/Sort
        Task<IEnumerable<Book>> SearchByAuthorAsync(string author);
        Task<IEnumerable<Book>> SortByPriceAsync(bool ascending);

        // Utilities
        Task<IEnumerable<Book>> GetRecentBooksAsync(int count);
    }
}