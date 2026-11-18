namespace LabCare.Models
{
    public class CultureFooterModel
    {
        public long cfcode { get; set; }

        public int orderno { get; set; }

        public string? shortname { get; set; }

        public string? name { get; set; }

        public string? description { get; set; }

        public bool deleted { get; set; } = false;

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime entereddate { get; set; }

        public DateTime ibsdate { get; set; }
        public string? tenant_code { get; set; }

    }
}