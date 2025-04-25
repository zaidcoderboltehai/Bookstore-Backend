using Bookstore.Business.Models;
using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Bookstore.Data.Interfaces;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bookstore.Business.Mappings;

namespace Bookstore.Business.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepo;
        private readonly IAdminRepository _adminRepo;

        public BookService(
            IBookRepository bookRepo,
            IAdminRepository adminRepo)
        {
            _bookRepo = bookRepo;
            _adminRepo = adminRepo;
        }

        #region CRUD Operations
        public async Task<Book> AddBookAsync(Book book)
        {
            ValidateBook(book);
            return await _bookRepo.AddAsync(book);
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
            => await _bookRepo.GetAllAsync();

        public async Task<Book?> GetBookByIdAsync(int id)
            => await _bookRepo.GetByIdAsync(id);

        public async Task UpdateBookAsync(Book book)
        {
            ValidateBook(book);
            await _bookRepo.UpdateAsync(book);
        }

        public async Task DeleteBookAsync(int id)
            => await _bookRepo.DeleteAsync(id);
        #endregion

        #region CSV Import (Updated Implementation)
        public async Task<int> ImportBooksFromCsvAsync(Stream fileStream, int adminId)
        {
            using var reader = new StreamReader(fileStream);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            csv.Context.RegisterClassMap<CsvBookMap>();
            csv.Context.TypeConverterCache.AddConverter<DateTime>(
                new DateTimeConverter("yyyy-MM-dd")
            );

            var books = new List<Book>();

            try
            {
                var records = csv.GetRecords<CsvBookDto>();

                foreach (var dto in records)
                {
                    // ✅ Admin Validation from CSV
                    var admin = await _adminRepo.GetByExternalId(dto.AdminUserId);
                    if (admin == null)
                        throw new Exception($"Admin not found: {dto.AdminUserId}");

                    var book = new Book
                    {
                        BookName = SanitizeString(dto.BookName),
                        Author = SanitizeString(dto.Author),
                        Description = SanitizeString(dto.Description),
                        Price = dto.Price,
                        DiscountPrice = dto.DiscountPrice,
                        Quantity = dto.Quantity,
                        BookImage = SanitizeString(dto.BookImage),
                        AdminId = admin.Id, // ✅ Use validated admin ID
                        CreatedAt = dto.CreatedAt,
                        UpdatedAt = dto.UpdatedAt
                    };

                    ValidateBook(book);
                    books.Add(book);
                }

                await _bookRepo.AddRangeAsync(books);
                return books.Count;
            }
            catch (CsvHelperException ex)
            {
                throw new ArgumentException($"CSV Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Import failed: {ex.Message}", ex);
            }
        }
        #endregion

        #region Search/Sort Methods
        public async Task<IEnumerable<Book>> SearchByAuthorAsync(string author)
            => await _bookRepo.SearchByAuthorAsync(author);

        public async Task<IEnumerable<Book>> SortByPriceAsync(bool ascending)
            => await _bookRepo.SortByPriceAsync(ascending);

        public async Task<IEnumerable<Book>> GetRecentBooksAsync(int count)
            => await _bookRepo.GetRecentAsync(count);
        #endregion

        #region Helpers
        private static string? SanitizeString(string? input)
            => string.IsNullOrWhiteSpace(input) ? null : input.Trim();

        private void ValidateBook(Book book)
        {
            if (book.Quantity < 0)
                throw new ArgumentException("Quantity cannot be negative");

            if (book.Price < 0)
                throw new ArgumentException("Price cannot be negative");

            if (string.IsNullOrWhiteSpace(book.BookName))
                throw new ArgumentException("Book name is required");
        }
        #endregion
    }

    #region CSV Converters
    public class DateTimeConverter : DefaultTypeConverter
    {
        private readonly string _format;

        public DateTimeConverter(string format) => _format = format;

        public override object ConvertFromString(
            string text,
            IReaderRow row,
            MemberMapData memberMapData)
        {
            if (DateTime.TryParseExact(text, _format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date))
                return date;

            throw new FormatException(
                $"Invalid date in row {row.Context.Parser.Row}: '{text}'. Expected format: {_format}");
        }
    }
    #endregion
}