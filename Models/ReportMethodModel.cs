using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Models
{
    [Table("report_method")]
    public class ReportMethodModel
    {
        [Key]
        public decimal rtmcode { get; set; }

        public string? tenant_code { get; set; }

        public int orderno { get; set; }

        public string? shortname { get; set; }

        [Required(ErrorMessage = "Method Name is required")]
        public string? name { get; set; }

        public int durationtime { get; set; }

        public string? duration { get; set; }

        public string? description { get; set; }

        public string? footer { get; set; }

        public bool deleted { get; set; } = false;

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime entereddate { get; set; } = DateTime.Now;

        public DateTime ibsdate { get; set; } = DateTime.Now;
    }
}
