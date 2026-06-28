using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Models
{
    [Table("group_master")]
    public class GroupMasterModel
    {
        [Key]
        public decimal gcode { get; set; }

        public string? tenant_code { get; set; }

        public decimal? dcode { get; set; }

        public int orderno { get; set; }

        [Required(ErrorMessage = "Group Name is required")]
        public string? name { get; set; }

        public string? shortname { get; set; }

        public string? description { get; set; }

        public string? footer { get; set; }

        public decimal? departmentcode { get; set; }

        public bool? isscan { get; set; }

        public bool? islab { get; set; }

        public bool deleted { get; set; } = false;

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime entereddate { get; set; } = DateTime.Now;

        public DateTime ibsdate { get; set; } = DateTime.Now;

        public bool? ischarges { get; set; }

        public bool? isinventory { get; set; }

        public bool? ispackage { get; set; }

        public bool? istreatment { get; set; }
    }
}
