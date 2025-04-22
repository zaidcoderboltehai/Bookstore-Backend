using System;
using System.Text.Json.Serialization;

namespace Bookstore.API.Models
{
    public class CsvBookDto
    {
        public string bookName { get; set; }
        public string author { get; set; }
        public string admin_user_id { get; set; } // ✅ CSV se Admin ID
        public string description { get; set; }
        public decimal price { get; set; }
        public decimal discountPrice { get; set; }
        public int quantity { get; set; }
        public string bookImage { get; set; }
        public CsvDate? createdAt { get; set; }
        public CsvDate? updatedAt { get; set; }
    }

    public class CsvDate
    {
        [JsonPropertyName("$date")]
        public DateTime Date { get; set; }
    }
}