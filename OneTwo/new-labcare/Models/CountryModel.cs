using Dapper.Contrib.Extensions;
using System.Text.Json.Serialization;

namespace LabCare.Models
{
    [Table("country_master")]
    public class CountryModel
    {
        [Key]
        [JsonPropertyName("countrycode")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int countrycode { get; set; }

        [JsonPropertyName("orderno")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int orderno { get; set; }

        [JsonPropertyName("shortname")]
        public string? shortname { get; set; }

        [JsonPropertyName("countryname")]
        public string countryname { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? description { get; set; }

        [JsonPropertyName("deleted")]
        public bool deleted { get; set; } = false;

        [JsonPropertyName("usercode")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int usercode { get; set; } = 1;

        [JsonPropertyName("computercode")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int computercode { get; set; } = 1;

        [JsonPropertyName("entereddate")]
        public DateTimeOffset entereddate { get; set; } = DateTimeOffset.Now;

        [JsonPropertyName("ibsdate")]
        public DateTimeOffset ibsdate { get; set; } = DateTimeOffset.Now;

        [JsonPropertyName("tenant_code")]
        public string tenant_code { get; set; } = string.Empty;
    }
}
