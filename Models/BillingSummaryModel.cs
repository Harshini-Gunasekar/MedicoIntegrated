using System;
using System.Collections.Generic;

namespace Booking.Models
{
    public class BillingSummaryCombinedResponse
    {
        public int custid { get; set; }
        public BillingSummaryVisitData? op { get; set; }
        public string? op_error { get; set; }
        public BillingSummaryVisitData? ip { get; set; }
        public string? ip_error { get; set; }
        public bool has_op { get; set; }
        public bool has_ip { get; set; }
    }

    public class BillingSummaryVisitData
    {
        public string? visit_type { get; set; }
        public string? id { get; set; }
        public string? number { get; set; }
        public int custid { get; set; }
        public string? patient_name { get; set; }
        public string? mobile { get; set; }
        public string? status { get; set; }
        public BillingSummaryVisitDetails? details { get; set; }
        public decimal unbilled_total { get; set; }
        public decimal billed_total { get; set; }
        public bool has_unbilled { get; set; }
        public bool has_billed { get; set; }
        public List<BillingSummaryChargeItem>? unbilled_items { get; set; } = new();
        public List<BillingSummaryChargeItem>? billed_items { get; set; } = new();
    }

    public class BillingSummaryVisitDetails
    {
        public int? dcode { get; set; }
        public string? visit_date { get; set; }
        public DateTime? admitdate { get; set; }
        public DateTime? dischargedate { get; set; }
    }

    public class BillingSummaryChargeItem
    {
        public string? unbilledid { get; set; }
        public string? entrytype { get; set; }
        public string? entryid { get; set; }
        public DateTime? chargedate { get; set; }
        public int? custid { get; set; }
        public string? opvisitid { get; set; }
        public string? ip_id { get; set; }
        public int? bedcode { get; set; }
        public int? tcode { get; set; }
        public decimal? quantity { get; set; }
        public decimal? rate { get; set; }
        public decimal? amount { get; set; }
        public decimal? discount { get; set; }
        public decimal? charityamount { get; set; }
        public bool? billedstatus { get; set; }
        public string? billno { get; set; }
        public string? billid { get; set; }
        public DateTime? billeddate { get; set; }
        public decimal? billedquantity { get; set; }
        public decimal? billedamount { get; set; }
        public string? particulars { get; set; }
        public string? tenant_code { get; set; }
        public bool? paid_status { get; set; }

        // Additional friendly display properties
        public string? item_name { get; set; }
    }

    public class BillingSummaryDisplayItem
    {
        public int SNo { get; set; }
        public string VisitType { get; set; } = "OP"; // OP or IP
        public string VisitNumber { get; set; } = ""; // OP No or IP No
        public string EntryType { get; set; } = ""; // CONSULTATION, INVESTIGATION, PROCEDURE, etc.
        public string Particulars { get; set; } = "";
        public DateTime? ChargeDate { get; set; }
        public decimal Quantity { get; set; } = 1;
        public decimal Rate { get; set; }
        public decimal Discount { get; set; }
        public decimal NetAmount { get; set; }
        public bool IsBilled { get; set; }
        public string? BillNo { get; set; }
        public string? BillId { get; set; }
        public DateTime? BilledDate { get; set; }
        public bool IsPaid { get; set; }
        public string? UnbilledId { get; set; }
        public string? RawEntryId { get; set; }
    }
}
