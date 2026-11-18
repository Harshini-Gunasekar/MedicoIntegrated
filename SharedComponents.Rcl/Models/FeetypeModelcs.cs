
namespace SharedComponents.Rcl.Models
{
    public class FeetypeModel
    {
        public int ftcode { get; set; }

        public int? orderno { get; set; }

        public string? shortname { get; set; }

        public string? name { get; set; }

        public string? description { get; set; }

        public double? commissionpercentage { get; set; }

        public bool deleted { get; set; }

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime entereddate { get; set; } = DateTime.Now;

        public DateTime ibsdate { get; set; } = DateTime.Now;

        public bool? ispatientrate { get; set; }

        public int? dcode { get; set; }

        public bool? iscommission { get; set; }

        public bool? isscheme { get; set; }

        public bool? isspecial { get; set; }

        public string? tenant_code { get; set; }
    }
}


