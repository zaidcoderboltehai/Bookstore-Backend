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
    [Authorize(Roles = "Admin")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private const int PageSize = 5; // Set page size to 5 as requested

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // CRUD Endpoints
        [HttpGet]
        public async Task<IActionResult> GetAllBooks()
            => Ok(await _bookService.GetAllBooksAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBook(int id)
            => Ok(await _bookService.GetBookByIdAsync(id));

        [HttpPost]
        public async Task<IActionResult> CreateBook(Book book)
        {
            var createdBook = await _bookService.AddBookAsync(book);
            return CreatedAtAction(nameof(GetBook), new { id = createdBook.Id }, createdBook);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBook(int id, Book book)
        {
            if (id != book.Id) return BadRequest();
            await _bookService.UpdateBookAsync(book);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            await _bookService.DeleteBookAsync(id);
            return NoContent();
        }

        // New Pagination Endpoint
        [HttpGet("page/{pageNumber}")]
        public async Task<IActionResult> GetBooksByPage(int pageNumber)
        {
            if (pageNumber < 1)
            {
                return BadRequest("Page number must be greater than 0");
            }

            var allBooks = await _bookService.GetAllBooksAsync();
            var totalBooks = allBooks.Count();
            var totalPages = (int)Math.Ceiling(totalBooks / (double)PageSize);

            var books = allBooks
                .Skip((pageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            return Ok(new
            {
                TotalBooks = totalBooks,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = PageSize,
                Books = books
            });
        }

        // Import books from CSV file
        [HttpPost("import")]
        public async Task<IActionResult> ImportBooks(IFormFile file)
        {
            var adminId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            using var stream = file.OpenReadStream();
            await _bookService.ImportBooksFromCsvAsync(stream, adminId);
            return Ok(new { Message = "Books imported successfully" });
        }

        // Search books by author
        [HttpGet("search")]
        public async Task<IActionResult> SearchByAuthor([FromQuery] string author)
        {
            var books = await _bookService.SearchByAuthorAsync(author);
            return Ok(books);
        }

        // Sort books by price
        [HttpGet("sorted")]
        public async Task<IActionResult> GetSorted([FromQuery] string sort = "price_asc")
        {
            var ascending = sort.Equals("price_asc", StringComparison.OrdinalIgnoreCase);
            var books = await _bookService.SortByPriceAsync(ascending);
            return Ok(books);
        }

        // Get recent books
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecent([FromQuery] int count = 5)
        {
            var books = await _bookService.GetRecentBooksAsync(count);
            return Ok(books);
        }
    }
}
