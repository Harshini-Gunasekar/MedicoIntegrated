using System;
using Dapper.Contrib.Extensions;

namespace LabCare.Models
{
    [Table("document_master")]
    public class document_master
    {
        [Key]  
        public int dmid { get; set; }

        public string dmname { get; set; } = string.Empty;
        public string? dmshortname { get; set; }
        public bool? isactive { get; set; } = true;
        public bool? deleted { get; set; } = false;
        public string? tenant_code { get; set; }
        public DateTimeOffset? entereddate { get; set; }
    }
}
