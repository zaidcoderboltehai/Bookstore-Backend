using Bookstore.Business.Interfaces;
using Bookstore.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Bookstore.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]  // Base authentication required
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private const int PageSize = 5;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // Get all books (Admin or User)
        [HttpGet]
        [Authorize(Roles = "ADMIN,USER")]
        public async Task<IActionResult> GetAllBooks()
            => Ok(await _bookService.GetAllBooksAsync());

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
            return NoContent();
        }

        // Delete book
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            await _bookService.DeleteBookAsync(id);
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
