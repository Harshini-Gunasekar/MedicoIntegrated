using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medico_backend.Model
{
    public class OpStatusResponse
    {
        public string status_code { get; set; } = string.Empty;
        public string default_status_name { get; set; } = string.Empty;
        public string display_status_name { get; set; } = string.Empty;
        public bool is_customized { get; set; }
    }

    public class OpStatusApiResponse<T>
    {
        public string status { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;
        public T? data { get; set; }
    }

    [Table("op_status_master")]
    public class OpStatusMasterModel
    {
        [Key]
        public int op_status_id { get; set; }

        [Required]
        [StringLength(100)]
        public string status_name { get; set; } = string.Empty;

        public string? tenant_code { get; set; }

        public bool is_active { get; set; } = true;

        public bool deleted { get; set; } = false;

        public int? usercode { get; set; }

        public bool is_fixed { get; set; }

        public string? status_key { get; set; }

        public int sort_order { get; set; }

        public string default_status_name { get; set; } = string.Empty;

        public string display_status_name { get; set; } = string.Empty;

        public bool is_customized { get; set; }

        public DateTime created_at { get; set; } = DateTime.UtcNow;

        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }
}
