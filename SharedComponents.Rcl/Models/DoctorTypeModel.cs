using System;
using System.Text.Json.Serialization;

namespace SharedComponents.Rcl.Models
{
    public class DoctorTypeModel
    {
        [JsonPropertyName("tcode")]
        public int tcode { get; set; }

        [JsonPropertyName("orderno")]
        public int? orderno { get; set; }

        [JsonPropertyName("name")]
        public string? name { get; set; }

        [JsonPropertyName("shortname")]
        public string? shortname { get; set; }

        [JsonPropertyName("description")]
        public string? description { get; set; }

        [JsonPropertyName("deleted")]
        public bool deleted { get; set; }

        [JsonPropertyName("usercode")]
        public int usercode { get; set; } = 1;

        [JsonPropertyName("computercode")]
        public int computercode { get; set; } = 1;

        [JsonPropertyName("entereddate")]
        public DateTime entereddate { get; set; } = DateTime.Now;

        [JsonPropertyName("ibsdate")]
        public DateTime ibsdate { get; set; } = DateTime.Now;

        [JsonPropertyName("tenant_code")]
        public string? tenant_code { get; set; }
    }
}


