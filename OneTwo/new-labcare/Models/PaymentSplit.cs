namespace LabCare.Models
{
    public class PaymentSplit
    {
        public int PaymodeCode { get; set; }
        public string PaymodeName { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string? ReferenceNo { get; set; } // For Card Ref
        public string? BankApp { get; set; }    // For GPay/PhonePe
    }
}
