using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace LabCare.Models
{
    [Table("container_master")] 
    public class ContainerMaster
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

        public string? container_image { get; set; }

        public bool? is_active { get; set; } = true;

        public DateTime? created_at { get; set; }

        public DateTime? ibsdate { get; set; }
    }

    [Table("container_details")]
    public class container_details
    {
        [Key]
        public int? cd_id { get; set; }

        public int container_id { get; set; }

        public int[]? sampleid { get; set; }

        public int[]? tcode { get; set; }

        public DateTimeOffset? entereddate { get; set; }

        public DateTimeOffset? ibsdate { get; set; }

        public int? usercode { get; set; }

        public string? tenant_code { get; set; }
    }

    public class ContainerInsertDto
    {
        public ContainerMaster ContainerMaster { get; set; } = new();
        public IList<container_details> ContainerDetails { get; set; } = new List<container_details>();
    }

    public class ContainerSuggestionDto
    {
        public int[]? tcodes { get; set; }
    }

    public class ContainerWithTcodes 
    { 
        public int? container_id { get; set; } 
        public string? tenant_code { get; set; } 
        public string? container_code { get; set; } 
        public string? container_name { get; set; } 
        public string? cap_color { get; set; } 
        public string? additive { get; set; } 
        public string? sample_type { get; set; } 
        public decimal? min_volume_ml { get; set; } 
        public string? description { get; set; } 
        public string? container_image { get; set; } 
        public bool? is_active { get; set; } 
        public DateTime? ibsdate { get; set; } 
        public DateTime? created_at { get; set; } 
        public int[]? tcode { get; set; } // from container_details 
    } 

    public class ContainerLinkSuggestDto
    {
        public int? container_id { get; set; }
        public string? container_code { get; set; }
        public string? container_name { get; set; }
        public string? cap_color { get; set; }
        public string? additive { get; set; }
        public string? sample_type { get; set; }
        public decimal? min_volume_ml { get; set; }
        public string? description { get; set; }
        public string? container_image { get; set; }
        public bool? is_active { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? ibsdate { get; set; }
        public string? tenant_code { get; set; }
    }
}
