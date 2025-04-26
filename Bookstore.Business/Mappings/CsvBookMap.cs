using Bookstore.Business.Models;
using CsvHelper.Configuration;

namespace Bookstore.Business.Mappings
{
    public sealed class CsvBookMap : ClassMap<CsvBookDto>
    {
        public CsvBookMap()
        {
            // ✅ CSV Columns -> CsvBookDto Properties Mapping
            Map(m => m.BookName).Name("bookName"); // PascalCase property
            Map(m => m.Author).Name("author");
            Map(m => m.AdminUserId).Name("admin_user_id"); // ✅ Snake-case column se map
            Map(m => m.Description).Name("description");
            Map(m => m.Price).Name("price");
            Map(m => m.DiscountPrice).Name("discountPrice");
            Map(m => m.Quantity).Name("quantity");
            Map(m => m.BookImage).Name("bookImage").Optional(); // Optional column
            Map(m => m.CreatedAt).Name("createdAt");
            Map(m => m.UpdatedAt).Name("updatedAt");
        }
    }
}