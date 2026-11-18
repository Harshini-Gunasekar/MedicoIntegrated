using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LabCare.Models
{
    // ══════════════════════════════════════════════════════════════
    //  PAYMENT MODE CONSTANTS  (pmc1 = Cash, pmc2 = Card, pmc3 = GPay)
    // ══════════════════════════════════════════════════════════════
    public static class PaymentMode
    {
        public const string Cash = "CASH";
        public const string Card = "CARD";
        public const string GPay = "GPAY";
    }

    // ══════════════════════════════════════════════════════════════
    //  REQUEST DTOs
    // ══════════════════════════════════════════════════════════════

    public class CreateBillDto
    {
        // ── Routing (create vs update) ────────────────────────────
        // null  → create new bill   (POST billing/create)
        // value → update existing   (POST billing/create with requestguid set)
        public Guid? requestguid { get; set; }

        // ── Patient ──────────────────────────────────────────────
        public long? custid { get; set; }
        public string? name { get; set; }
        public string? gender { get; set; }
        public string? dateofbirth { get; set; }
        public string? ageyears { get; set; }
        public string? agemonths { get; set; }
        public string? agedays { get; set; }
        public string? mobileno { get; set; }
        public string? address { get; set; }
        public int? areacode { get; set; }

        public int? doc1_dmid { get; set; }      // FK → document_master.dmid
        public string? doc1_dmvalue { get; set; } // e.g. "6190 4411 2233"

        public int? doc2_dmid { get; set; }      // FK → document_master.dmid
        public string? doc2_dmvalue { get; set; }
        public string? doc1_path { get; set; }
        public string? doc2_path { get; set; }

        // ── Counter / Branch / Series ────────────────────────────
        public long? bncode { get; set; }
        public long? cntcode { get; set; }
        public Guid? cnttid { get; set; }   // auto-resolved if omitted
        public long? tmcode { get; set; }   // auto-resolved if omitted
        public int? enteredbhcode { get; set; }

        // ── References ───────────────────────────────────────────
        public long? dcode { get; set; }
        public long? ricode { get; set; }
        public int? ftcode { get; set; }
        public long? ctcode { get; set; }
        public long? dlcode { get; set; }
        public long? pmcode { get; set; }
        public long? icode { get; set; }

        // ── Amounts ──────────────────────────────────────────────
        public double? requestamount { get; set; }
        public double? discountper { get; set; }
        public double? discountamount { get; set; }
        public double? totalamount { get; set; }
        public double? paidamount { get; set; }
        public double? specialdiscount { get; set; }
        public double? ourdispercentage { get; set; }
        public double? ourdiscount { get; set; }

        // ── Split Payment (pmc1=Cash, pmc2=Card, pmc3=GPay) ─────
        public double? pmc1 { get; set; }   // Cash
        public double? pmc2 { get; set; }   // Card
        public double? pmc3 { get; set; }   // GPay

        // ── Flags ────────────────────────────────────────────────
        public bool? iscashbill { get; set; }
        public bool? iscreditbill { get; set; }
        public string? billtype { get; set; }
        public bool? ishomecollection { get; set; }
        public int? homecollectionareacode { get; set; }
        public double? homecollectioncharge { get; set; }
        public string? concessionreason { get; set; }
        public string? card_refno { get; set; }
        public string? bank_app { get; set; }

        // ── Secondary Doctor / Hospital ──────────────────────────
        public int? seconddcode { get; set; }
        public string? seconddoctorname { get; set; }
        public string? hospitalid { get; set; }
        public string? fronthospitalid { get; set; }
        public string? fronthospitalpatientid { get; set; }
        public string? hospitalpatientid { get; set; }
        public string? careoftitile { get; set; }
        public string? careofinitial { get; set; }
        public string? careof { get; set; }

        public string? preferredlanguage { get; set; }
        public string? occupation { get; set; }
        public bool? isvip { get; set; }

        // ── Meta ─────────────────────────────────────────────────
        public int? usercode { get; set; }
        public int? computercode { get; set; }
        public string? collection_type { get; set; }   // fallback if pmc1/2/3 not used
        public Guid? receipt_guid { get; set; }   // idempotency key (create only)

        // ── Test Items ───────────────────────────────────────────
        public List<BillTestItemDto> testitems { get; set; } = new();
    }

    public class BillTestItemDto
    {
        public long? tcode { get; set; }
        public int? packcode { get; set; }
        public double? testamount { get; set; }
        public double? standardprice { get; set; }
        public double? testrate { get; set; }
        public double? discount { get; set; }
        public double? collection { get; set; }
        public int? qty { get; set; }
        public bool? ispack { get; set; }
        public int? ttid { get; set; }
    }

    /// <summary>
    /// Kept for backward compatibility. New code should use <see cref="CreateBillDto"/> with
    /// <c>requestguid</c> set — the backend's <c>billing/create</c> endpoint handles both
    /// create (requestguid=null) and update (requestguid=value) in one call.
    /// </summary>
    [Obsolete("Use CreateBillDto with requestguid set instead. POST billing/create handles both create and update.")]
    public class UpdateBillDto
    {
        public Guid? requestguid { get; set; }
        public long? dcode { get; set; }
        public int? ftcode { get; set; }
        public long? pmcode { get; set; }
        public double? discountper { get; set; }
        public double? discountamount { get; set; }
        public double? specialdiscount { get; set; }
        public double? paidamount { get; set; }
        public bool? iscashbill { get; set; }
        public bool? iscreditbill { get; set; }
        public string? billtype { get; set; }
        public string? concessionreason { get; set; }
        public string? card_refno { get; set; }
        public string? bank_app { get; set; }
        public int? seconddcode { get; set; }
        public string? seconddoctorname { get; set; }
        public int? usercode { get; set; }
        public int? computercode { get; set; }
        public int? doc1_dmid { get; set; }
        public string? doc1_dmvalue { get; set; }
        public int? doc2_dmid { get; set; }
        public string? doc2_dmvalue { get; set; }
        public string? doc1_path { get; set; }
        public string? doc2_path { get; set; }
        public List<BillTestItemDto> testitems { get; set; } = new();
    }

    public class BillFilterDto
    {
        public DateTime? fromdate { get; set; }
        public DateTime? todate { get; set; }
        public long? custid { get; set; }
        public long? dcode { get; set; }
        public int? ftcode { get; set; }
        public long? pmcode { get; set; }
        public bool? iscashbill { get; set; }
        public bool? iscreditbill { get; set; }
        public bool? pendingonly { get; set; }
        public bool? resultready { get; set; }
        public int? bhcode { get; set; }
        public int? requestsno { get; set; }
        public string? search { get; set; }

        // New filters
        public long? cntcode { get; set; }   // Counter code
        public long? bncode { get; set; }    // Bill number config code (bncode from lab_request_master)

        public int page { get; set; } = 1;
        public int pagesize { get; set; } = 50;
    }

    /// <summary>
    /// Close counter by cnttid OR by (bhcode + cntcode) — tenant_code is taken from request header.
    /// </summary>
    public class CloseCounterDto
    {
        public Guid? cnttid { get; set; }   // Option A: direct close by shift GUID
        public int? usercode { get; set; }
    }


    // ══════════════════════════════════════════════════════════════
    //  RESPONSE DTOs
    // ══════════════════════════════════════════════════════════════

    public class BillResponseDto
    {
        // ── Bill / Request identity ──────────────────────────────────────────
        public Guid? requestguid { get; set; }
        public int? requestsno { get; set; }
        public string? requestsnoprint { get; set; }
        public string? requestbarcode { get; set; }
        public string? billnoprint { get; set; }
        public int? billno { get; set; }
        public string? pathologyno { get; set; }
        public DateTimeOffset? requestdatetime { get; set; }

        // ── Patient demographics ─────────────────────────────────────────────
        public long? custid { get; set; }
        public string? prefix { get; set; }   // customer_master.nametitile
        public string? initial { get; set; }   // customer_master.initial
        public string? patientname { get; set; }   // lab_request_master.name
        public string? careof { get; set; }   // customer_master.careof
        public string? careoftitle { get; set; }   // customer_master.careoftitile
        public string? gender { get; set; }
        public string? dateofbirth { get; set; }
        public string? ageyears { get; set; }
        public string? agemonths { get; set; }
        public string? agedays { get; set; }
        public string? address { get; set; }
        public string? mobileno { get; set; }
        public string? email { get; set; }   // customer_master.email

        // ── Referral / clinical ──────────────────────────────────────────────
        public string? doctorname { get; set; }
        public int? seconddcode { get; set; }
        public string? seconddoctorname { get; set; }
        public string? feetype { get; set; }
        public string? paymode { get; set; }
        public string? countername { get; set; }
        public int? enteredbhcode { get; set; }

        // ── Financial summary ────────────────────────────────────────────────
        public double? requestamount { get; set; }
        public double? discountamount { get; set; }
        public double? specialdiscount { get; set; }
        public double? totalamount { get; set; }
        public double? paidamount { get; set; }
        public double? balanceamount { get; set; }
        public bool? issettled { get; set; }
        public double? pmc1 { get; set; }
        public double? pmc2 { get; set; }
        public double? pmc3 { get; set; }
        public string? concessionreason { get; set; }

        // ── Home collection ──────────────────────────────────────────────────
        public bool? ishomecollection { get; set; }
        public double? homecollectioncharge { get; set; }

        // ── Payment / card ref ───────────────────────────────────────────────
        public string? card_refno { get; set; }
        public string? bank_app { get; set; }

        // ── Receipt ─────────────────────────────────────────────────────────
        public Guid? receiptguid { get; set; }
        public string? receiptbarcode { get; set; }
        public string? receiptsnoprint { get; set; }

        // ── Counter / tenant refs ────────────────────────────────────────────
        public Guid? cnttid { get; set; }
        public long? tmcode { get; set; }

        // ── Documents ────────────────────────────────────────────────────────
        public int? doc1_dmid { get; set; }
        public string? doc1_dmvalue { get; set; }
        public string? doc1_path { get; set; }
        public int? doc2_dmid { get; set; }
        public string? doc2_dmvalue { get; set; }
        public string? doc2_path { get; set; }

        // ── Additional compatibility properties ──────────────────────────────
        public long? dcode { get; set; }
        public double? discountper { get; set; }
        public double? refconper { get; set; }
        public double? refconamount { get; set; }
        public string? enteredby { get; set; }
        public string? username { get; set; }
        public string? enteredby_name { get; set; }
        public int? areacode { get; set; }
        public string? areaname { get; set; }
        public bool? isvip { get; set; }
        public long? cntcode { get; set; }
        public int? homecollectionareacode { get; set; }
        public double? ourdispercentage { get; set; }
        public double? ourdiscount { get; set; }
        public string? patientid { get; set; }
        public string? careoftitile { get; set; }
        public string? careofinitial { get; set; }
        public string? preferredlanguage { get; set; }
        public string? occupation { get; set; }

        // ── Test line items ──────────────────────────────────────────────────
        public List<BillDetailLineDto> testitems { get; set; } = new();
    }

    // ─── Per-test line on the bill ────────────────────────────────────────────
    public class BillDetailLineDto
    {
        // ── From lab_request_details ─────────────────────────────────────────
        public Guid requestdetailsid { get; set; }
        public Guid? requestguid { get; set; }
        public int? testsno { get; set; }
        public double? testamount { get; set; }
        public double? finalamount { get; set; }
        public double? discount { get; set; }
        public double? standardprice { get; set; }   // lab_request_details
        public double? collection { get; set; }   // lab_request_details
        public int? qty { get; set; }
        public double? testrate { get; set; }
        public double? refundamount { get; set; }   // lab_request_details
        public bool? ispack { get; set; }   // lab_request_details

        // ── Result / authorisation status ────────────────────────────────────
        public bool? resultstatus { get; set; }
        public bool? istesthavingresult { get; set; }
        public bool? islockresult { get; set; }
        public bool? isauthorized1 { get; set; }
        public bool? isauthorized2 { get; set; }
        public bool? isprinted { get; set; }
        public bool? isworklistprinted { get; set; }   // lab_request_details
        public int? resultenteredby { get; set; }   // lab_request_details
        public int? resultauthorizedby { get; set; }   // lab_request_details
        public int? resultauthorizedby2 { get; set; }   // lab_request_details
        public DateTimeOffset? resultentereddate { get; set; }   // lab_request_details
        public DateTimeOffset? firstauthorizedate { get; set; }   // lab_request_details
        public DateTimeOffset? secondauthorizedate { get; set; }   // lab_request_details

        // ── From test_master (via TestMapInfo lookup) ─────────────────────────
        public long? tcode { get; set; }
        public string? testname { get; set; }   // test_master.name
        public string? shortname { get; set; }   // test_master.shortname
        public int? ttid { get; set; }   // test_master.ttid (replaces isculture / routine / outlab)
        public string? test_type_name { get; set; }   // test_master.test_type_name (computed)
        public bool? ispackage { get; set; }   // test_master.ispackage
        public int? packcode { get; set; }   // test_master.packcode
        public decimal? tax_rate { get; set; }   // test_master.tax_rate
        public string? icd_code { get; set; }   // test_master.icd_code
    }

    public class BillSummaryDto
    {
        public Guid? requestguid { get; set; }
        public int? requestsno { get; set; }
        public string? requestsnoprint { get; set; }
        public string? requestbarcode { get; set; }
        public string? billnoprint { get; set; }
        public string? patientname { get; set; }
        public string? mobileno { get; set; }
        public double? totalamount { get; set; }
        public double? discountper { get; set; }
        public double? discountamount { get; set; }
        public double? paidamount { get; set; }
        public double? balanceamount { get; set; }
        public bool? issettled { get; set; }
        public bool? resultstatus { get; set; }
        public DateTimeOffset? requestdatetime { get; set; }
        public int? enteredbhcode { get; set; }
        public string? doctorname { get; set; }
        public string? domain { get; set; }
        public string? username { get; set; }
        public int? doc1_dmid { get; set; }
        public string? doc1_dmvalue { get; set; }
        public int? doc2_dmid { get; set; }
        public string? doc2_dmvalue { get; set; }

    }

    public class CounterTimingDto
    {
        public Guid cnttid { get; set; }
        public long? cntcode { get; set; }
        public string? countername { get; set; }
        public string? domain { get; set; }
        public int? bhcode { get; set; }
        public int? shiftsno { get; set; }
        public DateTime? counterdate { get; set; }
        public DateTime? fromdate { get; set; }
        public DateTime? todate { get; set; }
        public bool is_open { get; set; }
        public bool is_closed { get; set; }
    }

    // ══════════════════════════════════════════════════════════════
    //  INTERNAL HELPERS
    // ══════════════════════════════════════════════════════════════

    internal class TestPriceInfo
    {
        public long tcode { get; set; }
        public string name { get; set; } = string.Empty;
        public string shortname { get; set; } = string.Empty;
        public double amount { get; set; }
    }

    internal class ResolvedTestItem
    {
        public long? tcode { get; set; }
        public int? packcode { get; set; }
        public double resolvedAmount { get; set; }
        public double? standardprice { get; set; }
        public double? testrate { get; set; }
        public double? discount { get; set; }
        public double? collection { get; set; }
        public int? qty { get; set; }
        public bool? ispack { get; set; }
        public int? ttid { get; set; }
    }

    internal class RequestSnoBag
    {
        public int requestsno { get; set; }
        public string requestsnoprint { get; set; } = string.Empty;
        public string requestbarcode { get; set; } = string.Empty;
        public long usedBncode { get; set; }
    }

    /// <summary>Represents a single split-payment entry (cash / card / gpay).</summary>
    internal record InternalPaymentSplit(double Amount, string CollectionType);

    // ─────────────────────────────────────────────────────────────────────────
    //  ADDITIONAL DTOs & UI MODELS
    // ─────────────────────────────────────────────────────────────────────────

    public class CollectBalanceDto
    {
        public Guid? requestguid { get; set; }
        public double collecting_amount { get; set; }
        public string? collection_type { get; set; }
        public long? pmcode { get; set; }
        public int? bhcode { get; set; }
        public Guid? receipt_guid { get; set; }
        public int? usercode { get; set; }
        public int? computercode { get; set; }
    }

    public class PaymentHistoryDto
    {
        public Guid balance_collection_by_id { get; set; }
        public string? collection_type { get; set; }
        public double? collected_amount { get; set; }
        public string? paymode { get; set; }
        public DateTimeOffset? collected_date { get; set; }
        public DateTimeOffset? entereddate { get; set; }
    }

    public class TenantResponseDto
    {
        public string? tenant_name { get; set; }
        public string? legal_name { get; set; }
        public string? address_line1 { get; set; }
        public string? address_line2 { get; set; }
        public string? city { get; set; }
        public string? pincode { get; set; }
        public string? contact_number { get; set; }
        public string? contact_email { get; set; }
        public string? website { get; set; }
        public string? logo_url { get; set; }
    }

    public class PaginatedBillingResponse
    {
        public int totalcount { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
        public int totalpage { get; set; }
        public List<BillResponseDto> data { get; set; } = new();
    }

    public class BillingModel
    {
        public string Branch { get; set; }
        public long? bhcode { get; set; }
        public string Counter { get; set; }
        public long? cntcode { get; set; }
        public string BillCatalog { get; set; }
        public string CaseNo { get; set; } 

        public int ftcode { get; set; } 
        public long ctcode { get; set; } 
        public long icode { get; set; } 
        public double specialdiscount { get; set; } = 0;
        public double ourdispercentage { get; set; } = 0;
        public double ourdiscount { get; set; } = 0;

        public long? custid { get; set; }
        public string PatientId { get; set; }
        public string SearchMobile { get; set; }
        public string Prefix { get; set; }
        public string PatientName { get; set; }
        public string Gender { get; set; }
        public DateTime? Dob { get; set; }
        public int? AgeY { get; set; }
        public int? AgeM { get; set; }
        public int? AgeD { get; set; }
        public string IdType1 { get; set; } 
        public int? doc1_dmid { get; set; }
        public string doc1_dmvalue { get; set; }
        public string IdType2 { get; set; }
        public int? doc2_dmid { get; set; }
        public string doc2_dmvalue { get; set; }
        public string? doc1_path { get; set; }
        public string? doc2_path { get; set; }
        public string Address { get; set; } 
        public int? areacode { get; set; }
        public string AreaName { get; set; }
        public string Email { get; set; } 
        public string? careoftitile { get; set; }
        public string? careofinitial { get; set; }
        public string? careof { get; set; } 

        public long? dcode { get; set; }
        public string PrimaryReferral { get; set; } 
        public string SecondaryReferral { get; set; }
        public long? seconddcode { get; set; }
        public string MonthlyAccount { get; set; }
        public string Scheme { get; set; }
        public string RoomNumber { get; set; } 
        public string HospitalIdType { get; set; } 
        public string HospitalId { get; set; } 
        public string HospitalPatientIdType { get; set; } 
        public string HospitalPatientId { get; set; } 

        public List<BillingTestItem> SelectedTests { get; set; } = new List<BillingTestItem>();
        public bool HomeCollection { get; set; } = false;

        public DateTime BillDate { get; set; } = DateTime.Now;
        public DateTime CollectedDate { get; set; } = DateTime.Now;

        public double BillAmount { get; set; } 
        public double GeneralConcessionPercent { get; set;}
        public double GeneralConcessionValue { get; set; }
        public double ReferralConcessionPercent { get; set; }
        public double ReferralConcessionValue { get; set; }
        public double DiscountPercent { get; set; }
        public double DiscountAmount { get; set; }
        public double NetPayable { get; set; } 

        public double CashAmount { get; set; } 
        public string PayMode { get; set; } 

        public double? pmc1 { get; set; } 
        public double? pmc2 { get; set; } 
        public double? pmc3 { get; set; } 
        
        public string? card_refno { get; set; }
        public string? bank_app { get; set; }
        
        public bool ishomecollection { get; set; } = false;
        public int? homecollectionareacode { get; set; }
        public double? homecollectioncharge { get; set; }

        public string? PreferredLanguage { get; set; }
        public string? Occupation { get; set; }
        public bool IsVIP { get; set; }
    }

    public class BillingTestItem
    {
        public int SNo { get; set; }
        public long? tcode { get; set; }
        public string TestDetails { get; set; } 
        public double BaseAmount { get; set; } 
        public double CommissionPercent { get; set; } 
        public double TotalAmount { get; set; } 
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WRAPPER FOR BILLING CREATION
    // ─────────────────────────────────────────────────────────────────────────
    public class createlabbillrequest
    {
        public lab_request_master labrequestmaster { get; set; } = new();
        public List<lab_request_details> labrequestdetails { get; set; } = new();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BILL PRINT DTOs (from GET billing/bill?requestguid=)
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Maps the response from GET billing/bill?requestguid={guid}.
    /// Contains cus_details (lab_request_master) and custest_details (lab_request_details[]).
    /// </summary>
    public class BillPrintDataDto
    {
        public BillPrintMasterDto? cus_details { get; set; }
        public List<BillPrintTestDetailDto>? custest_details { get; set; }
    }

    public class BillPrintMasterDto
    {
        public Guid? requestguid { get; set; }
        public int? requestsno { get; set; }
        public string? requestsnoprint { get; set; }
        public string? requestbarcode { get; set; }
        public string? requestconvertedbarcode { get; set; }
        public DateTimeOffset? requestdatetime { get; set; }
        public DateTimeOffset? requesteddatetime { get; set; }
        public DateTimeOffset? collecteddatetime { get; set; }
        public long? bncode { get; set; }
        public long? cntcode { get; set; }
        public Guid? cnttid { get; set; }
        public long? tmcode { get; set; }
        public long? custid { get; set; }
        public long? dcode { get; set; }
        public long? ricode { get; set; }
        public int? packcode { get; set; }
        public int? ftcode { get; set; }
        public int? ctcode { get; set; }
        public int? dlcode { get; set; }
        public int? pmcode { get; set; }
        public int? icode { get; set; }
        public string? name { get; set; }
        public string? gender { get; set; }
        public string? dateofbirth { get; set; }
        public string? ageyears { get; set; }
        public string? agemonths { get; set; }
        public string? agedays { get; set; }
        public string? address { get; set; }
        public int? areacode { get; set; }
        public string? mobileno { get; set; }
        public double? requestamount { get; set; }
        public double? discountper { get; set; }
        public double? discountamount { get; set; }
        public double? totalamount { get; set; }
        public double? paidamount { get; set; }
        public double? paidviareceipt { get; set; }
        public string? billtype { get; set; }
        public double? pmc1 { get; set; }
        public double? pmc2 { get; set; }
        public double? pmc3 { get; set; }
        public bool? iscashbill { get; set; }
        public bool? iscreditbill { get; set; }
        public double? newamount { get; set; }
        public double? homecollectioncharge { get; set; }
        public bool? ishomecollection { get; set; }
        public int? enteredbhcode { get; set; }
        public string? card_refno { get; set; }
        public string? bank_app { get; set; }
        public string? tenant_code { get; set; }
    }

    public class BillPrintTestDetailDto
    {
        public Guid? requestdetailsid { get; set; }
        public Guid? requestguid { get; set; }
        public int? testsno { get; set; }
        public int? packcode { get; set; }
        public long? tcode { get; set; }
        public double? testamount { get; set; }
        public double? discount { get; set; }
        public double? collection { get; set; }
        public int? ttid { get; set; }
        public double? standardprice { get; set; }
        public bool? ispack { get; set; }
        public int? qty { get; set; }
        public double? testrate { get; set; }
        public double? refundamount { get; set; }
        public string? tenant_code { get; set; }
    }
}
