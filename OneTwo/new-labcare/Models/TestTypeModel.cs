using Dapper.Contrib.Extensions;
using System;

namespace LabCare.Models
{
    [Table("test_type_master")]
    public class test_type_master
    {
        [Key] // BIGSERIAL — DB auto-increment
        public long ttid { get; set; }
        public string shortname { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public bool? deleted { get; set; } = false;
        public int? usercode { get; set; }
        public DateTimeOffset? entereddate { get; set; }
        public DateTimeOffset? ibsdate { get; set; }
        // '0' = global data visible to all tenants
        public string? tenant_code { get; set; }
    }
}
