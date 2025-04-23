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
using System.Text;
using System.Threading.Tasks;

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

        // CRUD Operations
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

        // Bulk Import with Enhanced CSV Handling
        public async Task ImportBooksFromCsvAsync(Stream fileStream, int currentAdminId)
        {
            using var reader = new StreamReader(fileStream, Encoding.UTF8);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                PrepareHeaderForMatch = args => args.Header.ToLower().Trim(),
                MissingFieldFound = null,
                HeaderValidated = null,
                IgnoreBlankLines = true,
                BadDataFound = context =>
                    Console.WriteLine($"Bad data at row {context.Context.Parser.Row}: {context.RawRecord}"),
                Delimiter = ",",
                Encoding = Encoding.UTF8
            };

            using var csv = new CsvReader(reader, config);
            csv.Context.TypeConverterCache.AddConverter<DateTime>(
                new DateTimeConverter("yyyy-MM-dd")
            );

            var books = new List<Book>();

            try
            {
                foreach (var dto in csv.GetRecords<CsvBookDto>())
                {
                    var book = ConvertToBookEntity(dto);
                    await ValidateAdminReference(book);
                    books.Add(book);
                }
            }
            catch (CsvHelperException ex)
            {
                throw new ArgumentException($"CSV processing failed: {ex.Message}");
            }

            foreach (var book in books)
            {
                await _bookRepo.AddAsync(book);
            }
        }

        // Search/Sort Features
        public async Task<IEnumerable<Book>> SearchByAuthorAsync(string author)
            => await _bookRepo.SearchByAuthorAsync(author);

        public async Task<IEnumerable<Book>> SortByPriceAsync(bool ascending)
            => await _bookRepo.SortByPriceAsync(ascending);

        public async Task<IEnumerable<Book>> GetRecentBooksAsync(int count)
            => await _bookRepo.GetRecentAsync(count);

        #region Private Helpers

        private Book ConvertToBookEntity(CsvBookDto dto)
        {
            return new Book
            {
                BookName = SanitizeString(dto.bookName),
                Author = SanitizeString(dto.author),
                Description = SanitizeString(dto.description),
                Price = dto.price,
                DiscountPrice = dto.discountPrice,
                Quantity = dto.quantity,
                BookImage = SanitizeString(dto.bookImage),
                AdminId = ResolveAdminId(dto.admin_user_id),
                CreatedAt = dto.createdAt,
                UpdatedAt = dto.updatedAt
            };
        }

        private int ResolveAdminId(string externalId)
        {
            var admin = _adminRepo.GetByExternalId(externalId).Result;
            return admin?.Id ?? throw new ArgumentException(
                $"Admin not found with External ID: {externalId}"
            );
        }

        private async Task ValidateAdminReference(Book book)
        {
            var admin = await _adminRepo.GetAdminByIdAsync(book.AdminId);
            if (admin == null)
                throw new ArgumentException($"Invalid Admin ID: {book.AdminId}");
        }

        private static string? SanitizeString(string? input)
            => string.IsNullOrWhiteSpace(input) ? null : input.Trim();

        private void ValidateBook(Book book)
        {
            if (book.Quantity < 0)
                throw new ArgumentException("Quantity cannot be negative");

            if (book.Price < 0)
                throw new ArgumentException("Price cannot be negative");
        }

        #endregion
    }

    #region CSV Configuration

    public class DateTimeConverter : DefaultTypeConverter
    {
        private readonly string _format;

        public DateTimeConverter(string format) => _format = format;

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            => DateTime.ParseExact(text, _format, CultureInfo.InvariantCulture);
    }

    #endregion
}