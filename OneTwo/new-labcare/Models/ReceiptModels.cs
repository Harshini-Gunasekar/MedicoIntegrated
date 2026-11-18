using Dapper.Contrib.Extensions;

namespace LIMS_Backend.Model
{
    [Table("receipt_master")]
    public class ReceiptMaster
    {
        [ExplicitKey]
        public Guid? receiptguid { get; set; }

        public DateTime? receiptdate { get; set; }

        public int? receiptsno { get; set; }

        public string? receiptsnoprint { get; set; }
        public string receiptbarcode { get; set; }

        public string receiptcovertedbarcode { get; set; }

        public int? cntcode { get; set; }

        public Guid? cnttid { get; set; }

        public long? tmcode { get; set; }

        public long? pmcode { get; set; }

        public long? ctcode { get; set; }

        public long? payledgercode { get; set; }

        public string bankname { get; set; }

        public string paymentreference { get; set; }

        public DateTime? chequedate { get; set; }

        public string cardno { get; set; }

        public DateTime? carddate { get; set; }

        public double? amountpaid { get; set; }

        public double? amountadjusted { get; set; }

        public double? amounttotal { get; set; }

        public bool? deleted { get; set; }

        public int? enteredbhcode { get; set; }

        public int? usercode { get; set; }

        public int? computercode { get; set; }

        public DateTime? entereddate { get; set; }

        public DateTime? ibsdate { get; set; }

        public bool? isbill { get; set; }

        public bool? ismonthly { get; set; }

        public bool? ispatient { get; set; }

        public bool? isrefferal { get; set; }

        public bool? isrefund { get; set; }

        public string? tenant_code { get; set; }
    }

    [Table("receipt_details")]
    public class ReceiptDetails
    {
        [ExplicitKey]
        public Guid? receipt_details_id { get; set; }

        public Guid? receiptguid { get; set; }

        public Guid? requestguid { get; set; }

        public double? receipt_amount { get; set; }

        public double? discount_amount { get; set; }

        public double? refund_amount { get; set; }

        public bool? deleted { get; set; }

        public int? usercode { get; set; }

        public int? computercode { get; set; }

        public DateTime? entereddate { get; set; }

        public DateTime? ibsdate { get; set; }

        public string? tenant_code { get; set; }
    }
}
