namespace ReportingServer.Model
{
    public class ReceiptModel
    {
    }
    public class ReceiptPdfModel
    {
        public string? ReceiptNo { get; set; }
        public DateTime ReceiptDate { get; set; }

        public int dcode { get; set; }

        public string? ReferralName { get; set; }
        public string? MobileNo { get; set; }
        public string? Address { get; set; }

        public string? BranchName { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string ? CompanyContactNo { get; set; }
        public string ? CompanyEmail { get; set; }

        //public string? BillNo { get; set; }

        public int? Totalbills { get; set; }

        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAmount { get; set; }

        public string? PaymentMode { get; set; }

    }

    public class ReceiptRequest
    {
        public ReceiptPdfModel? ReceiptData { get; set; }

        public byte[]? LogoImage { get; set; }
    }
}
