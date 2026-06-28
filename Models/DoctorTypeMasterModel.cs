using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Models
{
    [Table("doctor_type_master")]
    public class DoctorTypeMasterModel
    {
        [Key]
        public int tcode { get; set; }

        public string? tenant_code { get; set; }

        public int orderno { get; set; }

        [Required(ErrorMessage = "Doctor Type Name is required")]
        public string? name { get; set; }

        public string? shortname { get; set; }

        public string? description { get; set; }

        public bool deleted { get; set; } = false;

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime entereddate { get; set; } = DateTime.Now;

        public DateTime ibsdate { get; set; } = DateTime.Now;
    }
}
