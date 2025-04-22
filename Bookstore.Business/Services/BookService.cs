using Bookstore.Business.Models;
using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Bookstore.Business.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepo;
        private readonly IAdminRepository _adminRepo; // ✅ Added for Admin lookup

        public BookService(
            IBookRepository bookRepo,
            IAdminRepository adminRepo) // ✅ Dependency Injection
        {
            _bookRepo = bookRepo;
            _adminRepo = adminRepo;
        }

        // CRUD Operations
        public async Task<Book> AddBookAsync(Book book)
        {
            if (book.Quantity < 0)
                throw new ArgumentException("Quantity cannot be negative");

            await _bookRepo.AddAsync(book);
            return book;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
            => await _bookRepo.GetAllAsync();

        public async Task<Book?> GetBookByIdAsync(int id)
            => await _bookRepo.GetByIdAsync(id);

        public async Task UpdateBookAsync(Book book)
            => await _bookRepo.UpdateAsync(book);

        public async Task DeleteBookAsync(int id)
            => await _bookRepo.DeleteAsync(id);

        // Bulk Import with Admin Validation ✅
        public async Task ImportBooksFromCsvAsync(Stream fileStream, int currentAdminId)
        {
            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower()
            });

            var books = new List<Book>();
            foreach (var dto in csv.GetRecords<CsvBookDto>())
            {
                // Validate Admin from CSV's admin_user_id
                var admin = await _adminRepo.GetByExternalId(dto.admin_user_id);
                if (admin == null)
                {
                    throw new ArgumentException(
                        $"Admin with ID '{dto.admin_user_id}' not found for book: {dto.bookName}"
                    );
                }

                // Validate Quantity
                if (dto.quantity < 0)
                {
                    throw new ArgumentException(
                        $"Invalid quantity ({dto.quantity}) for book: {dto.bookName}"
                    );
                }

                // Create Book with resolved AdminId
                books.Add(new Book
                {
                    BookName = dto.bookName,
                    Author = dto.author,
                    Description = dto.description,
                    Price = dto.price,
                    DiscountPrice = dto.discountPrice,
                    Quantity = dto.quantity,
                    BookImage = dto.bookImage,
                    AdminId = admin.Id, // ✅ Use CSV's admin ID
                    CreatedAt = dto.createdAt?.Date ?? DateTime.UtcNow,
                    UpdatedAt = dto.updatedAt?.Date ?? DateTime.UtcNow
                });
            }

            // Bulk Insert
            foreach (var book in books)
            {
                await _bookRepo.AddAsync(book);
            }
        }

        // Search/Sort
        public async Task<IEnumerable<Book>> SearchByAuthorAsync(string author)
            => await _bookRepo.SearchByAuthorAsync(author);

        public async Task<IEnumerable<Book>> SortByPriceAsync(bool ascending)
            => await _bookRepo.SortByPriceAsync(ascending);

        // Utilities
        public async Task<IEnumerable<Book>> GetRecentBooksAsync(int count)
            => await _bookRepo.GetRecentAsync(count);
    }
}