using Dapper.Contrib.Extensions;
using System.Text.Json.Serialization;

namespace LabCare.Models
{
    [Table("city_master")]
    public class CityModel
    {
        [Key]
        [JsonPropertyName("citycode")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int citycode { get; set; }

        [JsonPropertyName("orderno")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int orderno { get; set; }

        [JsonPropertyName("shortname")]
        public string shortname { get; set; } = string.Empty;

        [JsonPropertyName("cityname")]
        public string cityname { get; set; } = string.Empty;

        [JsonPropertyName("statecode")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? statecode { get; set; }

        [JsonPropertyName("countrycode")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? countrycode { get; set; }

        [JsonPropertyName("tenant_code")]
        public string tenant_code { get; set; } = string.Empty;

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
    }
}
