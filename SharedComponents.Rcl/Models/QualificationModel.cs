using System.Text.Json.Serialization;

namespace SharedComponents.Rcl.Models
{
    public class QualificationModel
    {
        [JsonPropertyName("qcode")]
        public long? qcode { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Order No is required")]
        [System.ComponentModel.DataAnnotations.Range(1, 9999)]
        [JsonPropertyName("orderno")]
        public int? orderno { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Short Name is required")]
        [JsonPropertyName("shortname")]
        public string? shortname { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Qualification Name is required")]
        [JsonPropertyName("name")]
        public string? name { get; set; }

        [JsonPropertyName("description")]
        public string? description { get; set; }

        [JsonPropertyName("deleted")]
        public bool? deleted { get; set; } = false;

        [JsonPropertyName("usercode")]
        public int usercode { get; set; }

        [JsonPropertyName("computercode")]
        public int? computercode { get; set; }

        [JsonPropertyName("entereddate")]
        public DateTime? entereddate { get; set; }

        [JsonPropertyName("ibsdate")]
        public DateTime? ibsdate { get; set; }

        [JsonPropertyName("tenant_code")]
        public string? tenant_code { get; set; }
    }
}


