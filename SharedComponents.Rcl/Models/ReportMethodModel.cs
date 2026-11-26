using System.Text.Json.Serialization;

namespace SharedComponents.Rcl.Models
{
    public class ReportMethodModel
    {
        [JsonPropertyName("rtmcode")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long rtmcode { get; set; }

        [JsonPropertyName("orderno")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int orderno { get; set; }

        [JsonPropertyName("shortname")]
        public string? shortname { get; set; }

        [JsonPropertyName("name")]
        public string? name { get; set; }

        [JsonPropertyName("durationtime")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int durationtime { get; set; }

        [JsonPropertyName("duration")]
        public string? duration { get; set; }

        [JsonPropertyName("description")]
        public string? description { get; set; }

        [JsonPropertyName("footer")]
        public string? footer { get; set; }

        [JsonPropertyName("isculture")]
        public bool isculture { get; set; }

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
