using System;
using Dapper.Contrib.Extensions;

namespace LIMS_Backend.Model
{
    [Table("group_master")]
    public class GroupModel
    {
        [Key]
        public long gcode { get; set; }

        public long? dcode { get; set; }
        public int orderno { get; set; }
        public string? name { get; set; }
        public string? shortname { get; set; }
        public string? description { get; set; }
        public string? footer { get; set; }
        public long? departmentcode { get; set; }
        public bool? isscan { get; set; }
        public bool? islab { get; set; }
        public bool deleted { get; set; } = false;
        public int usercode { get; set; } = 1;
        public int computercode { get; set; } = 1;
        public DateTime entereddate { get; set; }
        public DateTime ibsdate { get; set; }
        public bool? ispackage { get; set; }
        public bool? ischarges { get; set; }
        public string? skycode { get; set; }
        public bool? isprintbarcode { get; set; }
        public int? workorderno { get; set; }
        public string? tenant_code { get; set; }

    }
}
