using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using System;
using System.Globalization;

namespace Bookstore.Business.Models
{
    public class CsvBookDto
    {
        // ✅ CSV Columns Mapping (PascalCase + CsvHelper Attributes)
        [Name("bookName")] // CSV column "bookName" se map hoga
        public string BookName { get; set; }

        [Name("author")]
        public string Author { get; set; }

        [Name("admin_user_id")] // Snake case column ke liye
        public string AdminUserId { get; set; }

        [Name("description")]
        public string Description { get; set; }

        [Name("price")]
        public decimal Price { get; set; }

        [Name("discountPrice")]
        public decimal DiscountPrice { get; set; }

        [Name("quantity")]
        public int Quantity { get; set; }

        [Name("bookImage")]
        public string BookImage { get; set; }

        [Name("createdAt")]
        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        [Name("updatedAt")]
        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime UpdatedAt { get; set; }
    }

    public class DateTimeConverter : DefaultTypeConverter
    {
        private const string Format = "yyyy-MM-dd";

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (DateTime.TryParseExact(text, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            throw new FormatException($"Invalid date in row {row.Context.Parser.Row}: '{text}'. Expected format: {Format}");
        }

        public override string ConvertToString(object value, IWriterRow row, MemberMapData memberMapData)
            => ((DateTime)value).ToString(Format);
    }
}