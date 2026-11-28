using Dapper.Contrib.Extensions;

namespace Medico_Backend.Model
{
    [Table("department_master")]
    public class DepartmentMasterModel
    {
        [Key]
        public decimal? dptcode { get; set; }

        public string? tenant_code { get; set; }

        public int orderno { get; set; }

        public string? name { get; set; }

        public string? shortname { get; set; }

        public string? description { get; set; }

        public string? footer { get; set; }

        public bool? isscan { get; set; }

        public bool? islab { get; set; }

        public bool deleted { get; set; } = false;

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime entereddate { get; set; }

        public DateTime ibsdate { get; set; }

        public bool? ischarges { get; set; }

        public bool? isinventory { get; set; }

        public bool? ispackage { get; set; }

        public bool? istreatment { get; set; }
    }

    public class DepartmentCollectionSummaryResponse
    {
        public DateTime from_date { get; set; }
        public DateTime to_date { get; set; }
        public int? filtered_bhcode { get; set; }
        public decimal? filtered_cntcode { get; set; }
        public decimal? filtered_dptcode { get; set; }

        public decimal total_gross_amount { get; set; }
        public decimal total_discount_amount { get; set; }
        public decimal total_net_amount { get; set; }
        public decimal total_collected_amount { get; set; }   // bill collections (real money)
        public decimal total_old_due_collected { get; set; }  // old bills paid in this period
        public decimal total_advance_used { get; set; }
        public decimal total_today_due { get; set; }          // balance of bills in this period
        public decimal total_overall_due { get; set; }        // outstanding up to to_date
        public int total_cancelled_count { get; set; }
        public decimal total_cancelled_amount { get; set; }

        public DepartmentAdvanceSummary advance_summary { get; set; } = new();
        public decimal total_advance_deposited { get; set; }
        public decimal total_advance_refunded { get; set; }
        public decimal total_net_cash_in { get; set; }   // collected + old due + deposited - refunded

        public List<DepartmentPaymodeSummary> paymode_summary { get; set; } = new();
        public List<DepartmentCollectionItem> departments { get; set; } = new();
    }

    public class DepartmentAdvanceSummary
    {
        public decimal deposited { get; set; }
        public decimal used { get; set; }
        public decimal refunded { get; set; }
        public decimal net_advance_movement { get; set; }   // deposited - refunded (cash side only)
        public int deposit_count { get; set; }
        public int used_bill_count { get; set; }
        public int refund_count { get; set; }
        public List<DepartmentPaymodeAmount> deposited_by_paymode { get; set; } = new();
        public List<DepartmentPaymodeAmount> refunded_by_paymode { get; set; } = new();
        public List<DepartmentAdvanceReceipt> deposits { get; set; } = new();
        public List<DepartmentAdvanceReceipt> refunds { get; set; } = new();
    }

    public class DepartmentPaymodeAmount
    {
        public string paymode { get; set; } = string.Empty;
        public decimal amount { get; set; }
        public int count { get; set; }
    }

    public class DepartmentAdvanceReceipt
    {
        public string? receipt_guid { get; set; }
        public string? receipt_no { get; set; }
        public DateTime? receipt_date { get; set; }
        public decimal? custid { get; set; }
        public string? patient_name { get; set; }
        public string? mobileno { get; set; }
        public string paymode { get; set; } = string.Empty;
        public string? reference_no { get; set; }
        public decimal amount { get; set; }
    }

    public class DepartmentCollectionItem
    {
        public decimal dptcode { get; set; }
        public string department_name { get; set; } = string.Empty;
        public decimal gross_amount { get; set; }
        public decimal discount_amount { get; set; }
        public decimal total_amount { get; set; }
        public decimal collected_amount { get; set; }
        public decimal old_due_collected { get; set; }
        public decimal total_cash_in { get; set; }            // collected + old due collected
        public decimal advance_used { get; set; }
        public decimal today_due { get; set; }
        public decimal overall_due { get; set; }
        public int cancelled_count { get; set; }
        public decimal cancelled_amount { get; set; }
        public List<DepartmentPaymodeSummary> paymodes { get; set; } = new();
        public List<DepartmentCancelledBill> cancelled_bills { get; set; } = new();
        public List<DepartmentGroupBreakdown> groups { get; set; } = new();
    }

    public class DepartmentPaymodeSummary
    {
        public string paymode { get; set; } = string.Empty;
        public decimal bill_amount { get; set; }
        public decimal old_due_amount { get; set; }
        public decimal amount { get; set; }                   // bill + old due
    }

    public class DepartmentCancelledBill
    {
        public string requestguid { get; set; } = string.Empty;
        public string? bill_no { get; set; }
        public DateTime? bill_date { get; set; }
        public string? patient_name { get; set; }
        public string? mobileno { get; set; }
        public string? reason { get; set; }
        public decimal amount { get; set; }                   // this department's share
    }

    public class DepartmentGroupBreakdown
    {
        public decimal gcode { get; set; }
        public string group_name { get; set; } = string.Empty;
        public decimal gross_amount { get; set; }
        public decimal discount_amount { get; set; }
        public decimal total_amount { get; set; }
        public decimal collected_amount { get; set; }
        public decimal advance_used { get; set; }
        public decimal due_amount { get; set; }
        public List<DepartmentTestBreakdown> tests { get; set; } = new();
    }

    public class DepartmentTestBreakdown
    {
        public decimal tcode { get; set; }
        public string test_name { get; set; } = string.Empty;
        public int billed_count { get; set; }
        public decimal gross_amount { get; set; }
        public decimal discount_amount { get; set; }
        public decimal total_amount { get; set; }
        public decimal collected_amount { get; set; }
        public decimal advance_used { get; set; }
        public decimal due_amount { get; set; }
    }
}