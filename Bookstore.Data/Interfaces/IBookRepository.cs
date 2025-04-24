using Bookstore.Data.Entities;

public interface IBookRepository
{
    Task<Book> AddAsync(Book book);
    Task<IEnumerable<Book>> GetAllAsync();
    Task<Book?> GetByIdAsync(int id);
    Task UpdateAsync(Book book);
    Task DeleteAsync(int id);
    Task<IEnumerable<Book>> SearchByAuthorAsync(string author);
    Task<IEnumerable<Book>> SortByPriceAsync(bool ascending = true);
    Task<IEnumerable<Book>> GetRecentAsync(int count);
}