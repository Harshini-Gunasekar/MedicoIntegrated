using System;
using System.Collections.Generic;

namespace Booking.Models
{
    public class HmsBillFilterRequest
    {
        public int? bhcode { get; set; }
        public int? cntcode { get; set; }
        public DateTime? fromdate { get; set; }
        public DateTime? todate { get; set; }
        public decimal? custid { get; set; }
        public int? dcode { get; set; }
        public bool? pendingonly { get; set; }
        public bool? iscashbill { get; set; }
        public bool? iscreditbill { get; set; }
        public string? search { get; set; }
        public int page { get; set; } = 1;
        public int pagesize { get; set; } = 20;
    }

    public class HmsBillSummary
    {
        public string? requestguid { get; set; }
        public string? bill_no { get; set; }
        public string? patient_name { get; set; }
        public string? mobileno { get; set; }
        public DateTime? bill_date { get; set; }
        public string? doctor_name { get; set; }
        public double? gross_amount { get; set; }
        public double? discount_amount { get; set; }
        public double? net_amount { get; set; }
        public double? paid_amount { get; set; }
        public double? balance_amount { get; set; }
        public bool is_settled { get; set; }
        public int? enteredbhcode { get; set; }
        public int? cntcode { get; set; }
    }

    public class HmsBillResponse
    {
        public string? requestguid { get; set; }
        public string? bill_no { get; set; }
        public string? barcode { get; set; }
        public DateTime? bill_date { get; set; }
        public decimal? custid { get; set; }
        public string? patient_name { get; set; }
        public string? gender { get; set; }
        public string? mobileno { get; set; }
        public string? ageyears { get; set; }
        public string? doctor_name { get; set; }
        public string? fee_type { get; set; }
        public string? pay_mode { get; set; }
        public string? counter_name { get; set; }
        public int? enteredbhcode { get; set; }
        public int? cntcode { get; set; }
        public string? cnttid { get; set; }
        public decimal? tmcode { get; set; }
        public double? gross_amount { get; set; }
        public double? discount_amount { get; set; }
        public double? general_concession_per { get; set; }
        public double? general_concession_amount { get; set; }
        public double? referral_concession_per { get; set; }
        public double? referral_concession_amount { get; set; }
        public double? tax_amount { get; set; }
        public double? net_amount { get; set; }
        public double? paid_amount { get; set; }
        public double? balance_amount { get; set; }
        public bool is_settled { get; set; }
        public double? pmc1 { get; set; }
        public double? pmc2 { get; set; }
        public double? pmc3 { get; set; }
        public string? receiptguid { get; set; }
        public string? receipt_no { get; set; }
        public string? receipt_barcode { get; set; }
        public List<HmsBillLineResponse> items { get; set; } = new();
    }

    public class HmsBillLineResponse
    {
        public string? requestdetailsid { get; set; }
        public string? charge_type { get; set; }
        public string? item_name { get; set; }
        public decimal? tcode { get; set; }
        public string? item_ref_id { get; set; }
        public double? unit_rate { get; set; }
        public double? amount { get; set; }
        public double? discount { get; set; }
        public double? final_amount { get; set; }
        public double? qty { get; set; }
        public double? gst_per { get; set; }
        public double? gst_amount { get; set; }
    }

    public class HmsBillListResponse
    {
        public int total { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public List<HmsBillSummary> data { get; set; } = new();
    }

    public class OpenShiftRequest
    {
        public int bhcode { get; set; }
        public int cntcode { get; set; }
        public DateTime counterdate { get; set; }
    }

    public class CloseShiftRequest
    {
        public string cnttid { get; set; }
        public int usercode { get; set; }
    }

    public class BillNoConfig
    {
        public int? bncode { get; set; }
        public string? name { get; set; }
        public string? shortname { get; set; }
        public int? orderno { get; set; }
        public int? bhcode { get; set; }
        public int? cntcode { get; set; }
        public bool isdefault { get; set; }
        public bool allbranch { get; set; }
        public bool allcounter { get; set; }
        public bool restartfinancialyear { get; set; }
        public bool restartcalendaryear { get; set; }
        public bool restartmonthly { get; set; }
        public bool restartdaily { get; set; }
        public bool issampleno { get; set; }
        public bool isreceiptno { get; set; }
        public bool deleted { get; set; }
        public string? tenant_code { get; set; }
        public DateTime? entereddate { get; set; }
        public int? sequence_rows_in_use { get; set; }
        public int usercode { get; set; } = 1;
        public int computercode { get; set; } = 1;
    }

    public class BillNoListRequest
    {
        public int page { get; set; } = 1;
        public int pagesize { get; set; } = 20;
    }

    public class BillNoListResponse
    {
        public int total { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public List<BillNoConfig> data { get; set; } = new();
    }

    public class CounterTimingDto
    {
        public Guid cnttid { get; set; }
        public int? cntcode { get; set; }
        public int? bhcode { get; set; }
        public int? shiftsno { get; set; }
        public DateTime? counterdate { get; set; }
        public DateTime? fromdate { get; set; }
        public DateTime? todate { get; set; }
        public string? tenant_code { get; set; }
        public int? usercode { get; set; }
        public int? computercode { get; set; }
    }
}
