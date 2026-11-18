using Dapper.Contrib.Extensions;
using System;

namespace LabCare.Models
{
    [Table("uom_master")]
    public class UomMaster
    {
        [Key]
        public long ucode { get; set; }

        public int orderno { get; set; }

        public string name { get; set; }

        public string shortname { get; set; }

        public int? decimalplaces { get; set; }

        public string? description { get; set; }

        public bool? deleted { get; set; } = false;

        public int usercode { get; set; }

        public int? computercode { get; set; }

        public DateTime? entereddate { get; set; }

        public DateTime? ibsdate { get; set; }

        public int? packsize { get; set; }

        public string? tenant_code { get; set; }
    }
}
