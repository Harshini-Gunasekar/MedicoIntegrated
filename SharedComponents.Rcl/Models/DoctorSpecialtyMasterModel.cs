using System;

namespace SharedComponents.Rcl.Models
{
    public class DoctorSpecialtyMasterModel
    {
        public long spcode { get; set; }

        public int orderno { get; set; } = 0;

        public string? name { get; set; }

        public string? shortname { get; set; }

        public string? description { get; set; }

        public bool deleted { get; set; } = false;

        public int usercode { get; set; } = 0;

        public int computercode { get; set; } = 0;

        public DateTime entereddate { get; set; } = DateTime.UtcNow;

        public DateTime ibsdate { get; set; } = DateTime.UtcNow;
        public string? tenant_code { get; set; }
    }
}


