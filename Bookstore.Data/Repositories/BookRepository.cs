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

        // ✅ CRUD Operations
        public async Task<Book> AddAsync(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }

        // 🆕 Bulk Add for CSV Import
        public async Task AddRangeAsync(IEnumerable<Book> books)
        {
            await _context.Books.AddRangeAsync(books);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
            => await _context.Books.ToListAsync();

        public async Task<Book?> GetByIdAsync(int id)
            => await _context.Books.FindAsync(id);

        public async Task UpdateAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
        }

        // ✅ Search/Sort/Utilities
        public async Task<IEnumerable<Book>> SearchByAuthorAsync(string author)
            => await _context.Books
                .Where(b => EF.Functions.Collate(b.Author, "SQL_Latin1_General_CP1_CS_AS") == author)
                .ToListAsync();

        public async Task<IEnumerable<Book>> SortByPriceAsync(bool ascending = true)
            => ascending
                ? await _context.Books.OrderBy(b => b.Price).ToListAsync()
                : await _context.Books.OrderByDescending(b => b.Price).ToListAsync();

        public async Task<IEnumerable<Book>> GetRecentAsync(int count)
            => await _context.Books
                .OrderByDescending(b => b.CreatedAt)
                .Take(count)
                .ToListAsync();
    }
}