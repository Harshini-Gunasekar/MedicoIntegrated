using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LabCare.Models
{
    [Table("container_master")]
    public class ContainerModel
    {
        [Key]
        public int? container_id { get; set; }

        public string? tenant_code { get; set; }

        public string? container_code { get; set; }

        public string container_name { get; set; } = string.Empty;

        public string? cap_color { get; set; }

        public string? additive { get; set; }

        public string? sample_type { get; set; }

        public decimal? min_volume_ml { get; set; }

        public string? description { get; set; }

        public byte[]? container_image { get; set; }

        public bool is_active { get; set; } = true;

        public DateTime? created_at { get; set; } = DateTime.Now;

        public DateTime? ibsdate { get; set; } = DateTime.Now;

        [JsonExtensionData]
        public IDictionary<string, JToken> UnmappedData { get; set; } = new Dictionary<string, JToken>();
    }
}