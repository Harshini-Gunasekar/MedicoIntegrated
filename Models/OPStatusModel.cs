using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace medico_backend.Model
{
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

        public DateTime created_at { get; set; } = DateTime.UtcNow;

        public DateTime updated_at { get; set; } = DateTime.UtcNow;
    }
}
