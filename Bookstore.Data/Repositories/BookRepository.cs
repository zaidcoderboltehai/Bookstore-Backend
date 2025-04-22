using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Data.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _context;

        public BookRepository(AppDbContext context) => _context = context;

        // Add new book
        public async Task<Book> AddAsync(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }

        // Get all books
        public async Task<IEnumerable<Book>> GetAllAsync()
            => await _context.Books.ToListAsync();

        // Get book by ID
        public async Task<Book?> GetByIdAsync(int id)
            => await _context.Books.FindAsync(id);

        // Update existing book
        public async Task UpdateAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        // Delete book by ID
        public async Task DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Correct Case-Sensitive Search
        public async Task<IEnumerable<Book>> SearchByAuthorAsync(string author)
        {
            return await _context.Books
                .Where(b => EF.Functions.Collate(b.Author, "SQL_Latin1_General_CP1_CS_AS") == author)
                .ToListAsync();
        }

        // Sort books by price (asc/desc)
        public async Task<IEnumerable<Book>> SortByPriceAsync(bool ascending = true)
            => ascending ?
                await _context.Books.OrderBy(b => b.Price).ToListAsync() :
                await _context.Books.OrderByDescending(b => b.Price).ToListAsync();

        // Get recent books (sorted by creation date)
        public async Task<IEnumerable<Book>> GetRecentAsync(int count)
            => await _context.Books
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();
    }
}