using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Models
{
    [Table("uom_master")]
    public class UomMasterModel
    {
        [Key]
        public decimal ucode { get; set; }

        public string? tenant_code { get; set; }

        public int orderno { get; set; }

        [Required(ErrorMessage = "Unit Name is required")]
        public string? name { get; set; }

        public string? shortname { get; set; }

        public int? decimalplaces { get; set; }

        public string? description { get; set; }

        public bool deleted { get; set; } = false;

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime entereddate { get; set; } = DateTime.Now;

        public DateTime ibsdate { get; set; } = DateTime.Now;

        public int? packsize { get; set; }
    }
}
