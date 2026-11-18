namespace LabCare.Models
{
    public class ReportingModel
    {
        public long recode { get; set; }

        public int orderno { get; set; }

        public string shortname { get; set; }

        public string name { get; set; }

        public string description { get; set; }

        public byte[] signatureimage { get; set; }

        public bool deleted { get; set; }

        public int usercode { get; set; }

        public int computercode { get; set; }

        public DateTime? entereddate { get; set; }

        public DateTime? ibsdate { get; set; }

        public string? tenant_code { get; set; }

    }
}