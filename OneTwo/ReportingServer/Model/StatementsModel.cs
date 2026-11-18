namespace ReportingServer.Model
{
    public class StatementsModel
    {
        public int sampleid { get; set; }
        public DateTime date { get; set; }
        public string custcode { get; set; }
        public string patientname { get; set; }
        public string? mobile { get; set; }
        public string? referral { get; set; }
        public decimal? billedamount { get; set; }
        public decimal? netamount { get; set; }
        public decimal? paidamount { get; set; }
        public decimal? balanceamount { get; set; }
        public decimal? discountamount { get; set; }
    }

    public class StatementRequest
    {
        public IList<StatementsModel>? statements { get; set; }
        public byte[]? LogoImage { get; set; }
        //Company Details
        public string? BranchName { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyAddress { get; set; }
        public string? CompanyContactNo { get; set; }
        public string? CompanyEmail { get; set; }
        public DateTime? fromdate { get; set; }
        public DateTime? todate { get; set; }
    }
}
