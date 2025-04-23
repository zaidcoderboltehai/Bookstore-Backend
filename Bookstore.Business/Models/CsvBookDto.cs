using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using CsvHelper.TypeConversion;
using System;
using System.Globalization;

namespace Bookstore.Business.Models
{
    public class CsvBookDto
    {
        public string bookName { get; set; }
        public string author { get; set; }
        public string admin_user_id { get; set; }
        public string description { get; set; }
        public decimal price { get; set; }
        public decimal discountPrice { get; set; }
        public int quantity { get; set; }
        public string bookImage { get; set; }

        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime createdAt { get; set; }

        [TypeConverter(typeof(DateTimeConverter))]
        public DateTime updatedAt { get; set; }
    }

    public class DateTimeConverter : DefaultTypeConverter
    {
        private const string Format = "yyyy-MM-dd";

        public override object ConvertFromString(string text, IReaderRow row,
            MemberMapData memberMapData)
        {
            if (DateTime.TryParseExact(text, Format, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var date))
            {
                return date;
            }

            throw new FormatException(
                $"Invalid date format in row {row.Context.Parser.Row}. " +
                $"Expected '{Format}', got '{text}'");
        }

        public override string ConvertToString(object value, IWriterRow row,
            MemberMapData memberMapData)
        {
            return ((DateTime)value).ToString(Format);
        }
    }
}