using System;
using System.Text.Json.Serialization;

namespace SharedComponents.Rcl.Models
{
    public class TestTypeModel
    {
        [JsonPropertyName("ttid")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public long ttid { get; set; }

        [JsonPropertyName("shortname")]
        public string shortname { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string name { get; set; } = string.Empty;

        [JsonPropertyName("deleted")]
        public bool? deleted { get; set; } = false;

        [JsonPropertyName("usercode")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? usercode { get; set; }

        [JsonPropertyName("entereddate")]
        public DateTimeOffset? entereddate { get; set; }

        [JsonPropertyName("ibsdate")]
        public DateTimeOffset? ibsdate { get; set; }

        [JsonPropertyName("tenant_code")]
        public string? tenant_code { get; set; }
    }
}
