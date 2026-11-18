using Dapper.Contrib.Extensions;
using System.ComponentModel.DataAnnotations;
using System;

namespace LabCare.Models
{
    [Table("paymode_master")]
    public class PayModeModel
    {
        [Dapper.Contrib.Extensions.Key]
        public int pmcode { get; set; }
        public int? orderno { get; set; }

        [Required(ErrorMessage = "Short name is required")]
        public string shortname { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required")]
        public string name { get; set; } = string.Empty;

        public int? durationtime { get; set; }
        public string? duration { get; set; }
        public string? description { get; set; }
        public string? footer { get; set; }
        public bool? deleted { get; set; }
        public int? usercode { get; set; }
        public int? computercode { get; set; }
        public DateTime? entereddate { get; set; }
        public DateTime? ibsdate { get; set; }
        public string? tenant_code { get; set; }
    }
}

