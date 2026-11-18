using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SharedComponents.Rcl.Models
{
    public class StateModel
    {
        [JsonPropertyName("statecode")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int statecode { get; set; }

        [JsonPropertyName("orderno")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? orderno { get; set; }

        [Required(ErrorMessage = "Short name is required")]
        [JsonPropertyName("shortname")]
        public string shortname { get; set; } = string.Empty;

        [Required(ErrorMessage = "State name is required")]
        [JsonPropertyName("statename")]
        public string statename { get; set; } = string.Empty;

        [JsonPropertyName("countrycode")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? countrycode { get; set; }

        [JsonPropertyName("tenant_code")]
        public string tenant_code { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? description { get; set; }

        [JsonPropertyName("deleted")]
        public bool? deleted { get; set; } = false;

        [JsonPropertyName("usercode")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? usercode { get; set; } = 1;

        [JsonPropertyName("computercode")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int? computercode { get; set; } = 1;

        [JsonPropertyName("entereddate")]
        public DateTimeOffset? entereddate { get; set; }

        [JsonPropertyName("ibsdate")]
        public DateTimeOffset? ibsdate { get; set; }
    }
}


