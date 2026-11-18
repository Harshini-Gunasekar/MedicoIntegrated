using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabCare.Models
{
    [Table("pro_master")]
    public class ProMasterModel
    {
        [Key]
        [JsonPropertyName("procode")]
        public int procode { get; set; }

        [Required(ErrorMessage = "Order No is required")]
        [Range(1, 9999, ErrorMessage = "Enter valid order no")]
        [JsonPropertyName("orderno")]
        public int orderno { get; set; }

        [Required(ErrorMessage = "Short Name is required")]
        [JsonPropertyName("shortname")]
        public string? shortname { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(100, MinimumLength = 3)]
        [JsonPropertyName("name")]
        public string? name { get; set; }

        [JsonPropertyName("address")]
        public string? address { get; set; }

        [JsonPropertyName("city")]
        public string? city { get; set; }

        [JsonPropertyName("pincode")]
        public string? pincode { get; set; }

        [JsonPropertyName("state")]
        public string? state { get; set; }

        [JsonPropertyName("country")]
        public string? country { get; set; }

        [JsonPropertyName("phone")]
        public string? phone { get; set; }

        [JsonPropertyName("contactname")]
        public string? contactname { get; set; }

        [Required(ErrorMessage = "Mobile No is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Enter valid 10-digit mobile")]
        [JsonPropertyName("mobile")]
        public string? mobile { get; set; }

        [JsonPropertyName("fax")]
        public string? fax { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email format")]
        [JsonPropertyName("email")]
        public string? email { get; set; }

        [JsonPropertyName("website")]
        public string? website { get; set; }

        [JsonPropertyName("description")]
        public string? description { get; set; }

        [JsonPropertyName("areacode")]
        public int? areacode { get; set; }

        [JsonPropertyName("deleted")]
        public bool deleted { get; set; }

        [JsonPropertyName("usercode")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int usercode { get; set; } = 1;

        [JsonPropertyName("computercode")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int computercode { get; set; } = 1;

        [JsonPropertyName("entereddate")]
        public DateTime entereddate { get; set; } = DateTime.Now;

        [JsonPropertyName("ibsdate")]
        public DateTime ibsdate { get; set; } = DateTime.Now;

        [JsonPropertyName("tenant_code")]
        public string tenant_code { get; set; } = string.Empty;
    }
}
