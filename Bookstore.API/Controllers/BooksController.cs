using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Bookstore.API.Services; // NEW: Redis aur RabbitMQ services

namespace Bookstore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Base authentication required
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;
        private readonly IRedisService _redisService; // NEW: Redis service
        private readonly IRabbitMQService _rabbitMQService; // NEW: RabbitMQ service
        private const int PageSize = 5;

        public BooksController(
            IBookService bookService,
            ILogger<BooksController> logger,
            IRedisService redisService, // NEW: Redis injection
            IRabbitMQService rabbitMQService) // NEW: RabbitMQ injection
        {
            _bookService = bookService;
            _logger = logger;
            _redisService = redisService;
            _rabbitMQService = rabbitMQService;
        }

        // Get all books (Admin or User)
        [HttpGet]
        [Authorize(Roles = "ADMIN,USER")]
        public async Task<IActionResult> GetAllBooks()
        {
            try
            {
                _logger.LogInformation("GetAllBooks API called");

                // NEW: Check Redis cache first
                var cacheKey = "all_books";
                var cachedBooks = await _redisService.GetAsync<object>(cacheKey);

                if (cachedBooks != null)
                {
                    _logger.LogInformation("Books retrieved from Redis cache");
                    return Ok(cachedBooks);
                }

                var books = await _bookService.GetAllBooksAsync();
                var booksList = books.ToList();

                var response = new
                {
                    Status = "Success",
                    Message = $"Retrieved {booksList.Count} books successfully",
                    Count = booksList.Count,
                    Data = booksList,
                    Timestamp = DateTime.UtcNow
                };

                // NEW: Cache the response in Redis for 5 minutes
                await _redisService.SetAsync(cacheKey, response, TimeSpan.FromMinutes(5));

                _logger.LogInformation("Retrieved {BookCount} books successfully and cached", booksList.Count);
                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Database error in GetAllBooks");
                return StatusCode(500, new
                {
                    Status = "Error",
                    ErrorCode = "BOOKS-DB-001",
                    Message = "Database error occurred",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in GetAllBooks");
                return StatusCode(500, new
                {
                    Status = "Error",
                    ErrorCode = "BOOKS-GEN-002",
                    Message = "An unexpected error occurred",
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        // Get single book by ID
        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN,USER")]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            return book != null
                ? Ok(book)
                : NotFound(new { Message = "Book not found" });
        }

        // Create new book
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateBook(Book book)
        {
            // Get admin ID from token
            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            book.AdminId = adminId; // ✅ Set from logged-in admin

            var createdBook = await _bookService.AddBookAsync(book);

            // NEW: Clear cache when new book is added
            await _redisService.DeleteAsync("all_books");

            // NEW: Send RabbitMQ notification
            _rabbitMQService.PublishBookAddedNotification(createdBook.Id, createdBook.BookName);

            return CreatedAtAction(nameof(GetBook), new { id = createdBook.Id }, createdBook);
        }

        // Update book
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateBook(int id, Book book)
        {
            if (id != book.Id)
                return BadRequest(new { Error = "ID mismatch between URL and body" });

            await _bookService.UpdateBookAsync(book);

            // NEW: Clear cache when book is updated
            await _redisService.DeleteAsync("all_books");

            return NoContent();
        }

        // Delete book
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            await _bookService.DeleteBookAsync(id);

            // NEW: Clear cache when book is deleted
            await _redisService.DeleteAsync("all_books");

            return NoContent();
        }

        // Paginated results
        [HttpGet("page/{pageNumber}")]
        [Authorize(Roles = "ADMIN,USER")]
        public async Task<IActionResult> GetBooksByPage(int pageNumber)
        {
            try
            {
                if (pageNumber < 1)
                    return BadRequest(new { Error = "Page number must be ≥ 1" });

                var allBooks = await _bookService.GetAllBooksAsync();
                var totalCount = allBooks.Count();
                var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

                var results = allBooks
                    .Skip((pageNumber - 1) * PageSize)
                    .Take(PageSize)
                    .Select(b => new {
                        b.Id,
                        b.BookName,
                        b.Author,
                        Price = $"₹{b.Price:N2}"
                    });

                return Ok(new
                {
                    TotalItems = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = pageNumber,
                    Books = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Server Error",
                    ex.Message
                });
            }
        }

        // CSV Import
        [HttpPost("import")]
        [Authorize(Roles = "ADMIN")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportBooks(
            [FromForm(Name = "file")] IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { Error = "Empty file uploaded" });

                if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                    return BadRequest(new { Error = "Only CSV files supported" });

                var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                using var stream = file.OpenReadStream();
                var importedCount = await _bookService.ImportBooksFromCsvAsync(stream, adminId);

                // NEW: Clear cache after bulk import
                await _redisService.DeleteAsync("all_books");

                return Ok(new
                {
                    Status = "Success",
                    ImportedCount = importedCount,
                    FileName = file.FileName,
                    FileSize = $"{file.Length / 1024} KB"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Status = "Import Failed",
                    ex.Message,
                    SupportedFormat = "CSV with columns: BookName,Author,Price,Quantity,AdminUserId"
                });
            }
        }

        // Search by author
        [HttpGet("search")]
        [Authorize(Roles = "ADMIN,USER")]
        public async Task<IActionResult> SearchByAuthor([FromQuery] string author)
        {
            if (string.IsNullOrWhiteSpace(author))
                return BadRequest(new { Error = "Search query required" });

            var results = await _bookService.SearchByAuthorAsync(author);
            return Ok(new
            {
                SearchTerm = author,
                ResultsCount = results.Count(),
                Books = results.Select(b => new { b.Id, b.BookName, b.Author })
            });
        }

        // Sort by price
        [HttpGet("sorted")]
        [Authorize(Roles = "ADMIN,USER")]
        public async Task<IActionResult> GetSorted([FromQuery] string sort = "price_asc")
        {
            try
            {
                var ascending = sort.Equals("price_asc", StringComparison.OrdinalIgnoreCase);
                var results = await _bookService.SortByPriceAsync(ascending);
                return Ok(new
                {
                    SortOrder = ascending ? "Low to High" : "High to Low",
                    Books = results.Select(b => new { b.Id, b.BookName, Price = $"₹{b.Price:N2}" })
                });
            }
            catch
            {
                return BadRequest(new { ValidValues = new[] { "price_asc", "price_desc" }, Example = "/api/books/sorted?sort=price_desc" });
            }
        }

        // Recent books
        [HttpGet("recent")]
        [Authorize(Roles = "ADMIN,USER")]
        public async Task<IActionResult> GetRecent([FromQuery] int count = 5)
        {
            if (count < 1 || count > 50)
                return BadRequest(new { Error = "Count must be between 1-50", Example = "/api/books/recent?count=10" });

            var results = await _bookService.GetRecentBooksAsync(count);
            return Ok(new
            {
                RecentCount = count,
                Books = results.Select(b => new { b.Id, b.BookName, AddedDate = b.CreatedAt.ToString("dd MMM yyyy") })
            });
        }
    }
}
