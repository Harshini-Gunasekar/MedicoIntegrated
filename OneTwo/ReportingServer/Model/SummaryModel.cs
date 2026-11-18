namespace ReportingServer.Model
{
    public class SummaryModel
    {
        public DateTime date { get; set; }
        public long? billed {  get; set; }
        public decimal? billedamount { get; set; }
        public decimal? paid { get; set; }
        public decimal? balance { get; set; }
        public decimal? discount { get; set; }
        public decimal? netamount { get; set; }
    }

    public class SummaryRequest
    {
        public IList<SummaryModel>? summary { get; set; }
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
