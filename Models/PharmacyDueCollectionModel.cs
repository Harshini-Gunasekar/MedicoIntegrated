using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Booking.Models
{
    // =========================================================
    //  PHARMACY DUE COLLECTION — MODELS & DTOS
    // =========================================================

    // ─── DUE COLLECTION MASTER ──────────────────────────────────────────────────
    public class due_collection_master
    {
        [JsonPropertyName("duecollectioncode")]
        public long duecollectioncode { get; set; }

        [JsonPropertyName("requestno")]
        public string? requestno { get; set; }

        [JsonPropertyName("requestdate")]
        public DateTime requestdate { get; set; } = DateTime.Now;

        [JsonPropertyName("salestype")]
        public string? salestype { get; set; }   // OP / IP / COUNTER / MIXED (NEW)

        [JsonPropertyName("patientid")]
        public string? patientid { get; set; }

        [JsonPropertyName("patientname")]
        public string? patientname { get; set; }

        [JsonPropertyName("mobileno")]
        public string? mobileno { get; set; }

        [JsonPropertyName("address")]
        public string? address { get; set; }

        [JsonPropertyName("custid")]
        public decimal? custid { get; set; }

        [JsonPropertyName("totalbills")]
        public int totalbills { get; set; }

        [JsonPropertyName("totaldueamount")]
        public decimal totaldueamount { get; set; }

        [JsonPropertyName("totalcollectedamount")]
        public decimal totalcollectedamount { get; set; }

        [JsonPropertyName("totalremainingamount")]
        public decimal totalremainingamount { get; set; }

        [JsonPropertyName("paymentmode")]
        public string? paymentmode { get; set; }

        [JsonPropertyName("chequeno")]
        public string? chequeno { get; set; }

        [JsonPropertyName("accountno")]
        public string? accountno { get; set; }

        [JsonPropertyName("referenceno")]
        public string? referenceno { get; set; }

        [JsonPropertyName("remarks")]
        public string? remarks { get; set; }

        [JsonPropertyName("status")]
        public string? status { get; set; }

        [JsonPropertyName("isactive")]
        public bool isactive { get; set; } = true;

        [JsonPropertyName("deleted")]
        public bool deleted { get; set; } = false;

        [JsonPropertyName("createddate")]
        public DateTime createddate { get; set; } = DateTime.Now;

        [JsonPropertyName("modifieddate")]
        public DateTime? modifieddate { get; set; }

        [JsonPropertyName("usercode")]
        public long? usercode { get; set; }

        [JsonPropertyName("tenantcode")]
        public string? tenantcode { get; set; }

        [JsonPropertyName("branchcode")]
        public string? branchcode { get; set; }

        [JsonPropertyName("companycode")]
        public string? companycode { get; set; }

        [JsonPropertyName("bh_code")]
        public int? bh_code { get; set; }

        [JsonPropertyName("cntcode")]
        public decimal? cntcode { get; set; }

        [JsonPropertyName("cnttid")]
        public string? cnttid { get; set; }
    }

    // ─── DUE COLLECTION DETAIL ──────────────────────────────────────────────────
    public class due_collection_detail
    {
        [JsonPropertyName("duecollectiondetailcode")]
        public long duecollectiondetailcode { get; set; }

        [JsonPropertyName("duecollectioncode")]
        public long duecollectioncode { get; set; }

        [JsonPropertyName("salescode")]
        public long salescode { get; set; }

        [JsonPropertyName("billno")]
        public string? billno { get; set; }

        [JsonPropertyName("billdate")]
        public DateTime? billdate { get; set; }

        [JsonPropertyName("salestype")]
        public string? salestype { get; set; }               // server (NEW)

        [JsonPropertyName("billamount")]
        public decimal billamount { get; set; }

        [JsonPropertyName("previouslypaidamount")]
        public decimal previouslypaidamount { get; set; }

        [JsonPropertyName("dueamount")]
        public decimal dueamount { get; set; }

        [JsonPropertyName("collectedamount")]
        public decimal collectedamount { get; set; }

        [JsonPropertyName("remainingamount")]
        public decimal remainingamount { get; set; }

        [JsonPropertyName("paymentstatus")]
        public string? paymentstatus { get; set; }

        [JsonPropertyName("remarks")]
        public string? remarks { get; set; }

        [JsonPropertyName("tenantcode")]
        public string? tenantcode { get; set; }
    }

    // ─── DUE COLLECTION REQUEST ─────────────────────────────────────────────────
    public class due_collection_request
    {
        [JsonPropertyName("master")]
        public due_collection_master master { get; set; } = new();

        [JsonPropertyName("details")]
        public List<due_collection_detail> details { get; set; } = new();
    }

    // ─── DUE PENDING BILL ───────────────────────────────────────────────────────
    public class due_pending_bill
    {
        [JsonPropertyName("salescode")]
        public long salescode { get; set; }

        [JsonPropertyName("billno")]
        public string? billno { get; set; }

        [JsonPropertyName("billdate")]
        public DateTime billdate { get; set; }

        [JsonPropertyName("salestype")]
        public string? salestype { get; set; }     // OP / IP / COUNTER (NEW)

        [JsonPropertyName("patientid")]
        public string? patientid { get; set; }

        [JsonPropertyName("patientname")]
        public string? patientname { get; set; }

        [JsonPropertyName("mobileno")]
        public string? mobileno { get; set; }

        [JsonPropertyName("billamount")]
        public decimal billamount { get; set; }

        [JsonPropertyName("paidamount")]
        public decimal paidamount { get; set; }

        [JsonPropertyName("remainingamount")]
        public decimal remainingamount { get; set; }

        [JsonPropertyName("paymentstatus")]
        public string? paymentstatus { get; set; }
    }

    // ─── BULK DUE COLLECTION (NEW) ───────────────────────────────────────────────
    public class due_bulk_item
    {
        [JsonPropertyName("salescode")]
        public long salescode { get; set; }

        [JsonPropertyName("collectedamount")]
        public decimal collectedamount { get; set; }

        [JsonPropertyName("remarks")]
        public string? remarks { get; set; }
    }

    public class due_bulk_collection_request
    {
        // ── which bills (filters, same as getpendingdues) ──
        [JsonPropertyName("salestype")]
        public string? salestype { get; set; }      // OP / IP / COUNTER / null = all

        [JsonPropertyName("patientid")]
        public string? patientid { get; set; }

        [JsonPropertyName("mobileno")]
        public string? mobileno { get; set; }

        [JsonPropertyName("patientname")]
        public string? patientname { get; set; }

        [JsonPropertyName("billno")]
        public string? billno { get; set; }

        [JsonPropertyName("fromdate")]
        public DateTime? fromdate { get; set; }

        [JsonPropertyName("todate")]
        public DateTime? todate { get; set; }

        [JsonPropertyName("salescodes")]
        public List<long> salescodes { get; set; } = new();   // optional: restrict to these bills

        // ── how much: pick ONE mode ──
        [JsonPropertyName("items")]
        public List<due_bulk_item> items { get; set; } = new();  // 1) explicit amount per bill

        [JsonPropertyName("collectfull")]
        public bool collectfull { get; set; }                    // 2) pay the full balance of every matched bill

        [JsonPropertyName("totalamount")]
        public decimal totalamount { get; set; }                 // 3) lump sum, allocated oldest bill first

        // ── payment info ──
        [JsonPropertyName("requestdate")]
        public DateTime? requestdate { get; set; }

        [JsonPropertyName("paymentmode")]
        public string? paymentmode { get; set; }

        [JsonPropertyName("chequeno")]
        public string? chequeno { get; set; }

        [JsonPropertyName("accountno")]
        public string? accountno { get; set; }

        [JsonPropertyName("referenceno")]
        public string? referenceno { get; set; }

        [JsonPropertyName("remarks")]
        public string? remarks { get; set; }

        [JsonPropertyName("usercode")]
        public long? usercode { get; set; }

        [JsonPropertyName("tenantcode")]
        public string? tenantcode { get; set; }

        [JsonPropertyName("branchcode")]
        public string? branchcode { get; set; }

        [JsonPropertyName("companycode")]
        public string? companycode { get; set; }

        [JsonPropertyName("bh_code")]
        public int? bh_code { get; set; }

        [JsonPropertyName("cntcode")]
        public decimal? cntcode { get; set; }

        [JsonPropertyName("cnttid")]
        public string? cnttid { get; set; }
    }

    public class due_bulk_collection_result
    {
        [JsonPropertyName("duecollectioncode")]
        public long duecollectioncode { get; set; }

        [JsonPropertyName("requestno")]
        public string? requestno { get; set; }

        [JsonPropertyName("salestype")]
        public string? salestype { get; set; }

        [JsonPropertyName("patientid")]
        public string? patientid { get; set; }

        [JsonPropertyName("patientname")]
        public string? patientname { get; set; }

        [JsonPropertyName("bills")]
        public int bills { get; set; }

        [JsonPropertyName("collected")]
        public decimal collected { get; set; }
    }

    // ─── FILTER FOR GET PENDING DUES ────────────────────────────────────────────
    public class PharmacyPendingDuesFilter
    {
        public string? tenantcode { get; set; }
        public string? patientid { get; set; }
        public string? mobileno { get; set; }
        public string? salestype { get; set; } // OP | IP | COUNTER
        public string? patientname { get; set; }
        public string? billno { get; set; }
        public DateTime? fromdate { get; set; }
        public DateTime? todate { get; set; }

        public bool HasAnyCriteria =>
            !string.IsNullOrWhiteSpace(patientid) ||
            !string.IsNullOrWhiteSpace(mobileno) ||
            !string.IsNullOrWhiteSpace(salestype) ||
            !string.IsNullOrWhiteSpace(patientname) ||
            !string.IsNullOrWhiteSpace(billno) ||
            fromdate.HasValue ||
            todate.HasValue;
    }

    // ─── COMPATIBILITY ALIASES & WRAPPERS ───────────────────────────────────────
    public class PharmacyPendingDueBill : due_pending_bill
    {
    }

    public class PharmacyDueCollectionMasterRequest : due_collection_master
    {
    }

    public class PharmacyDueCollectionDetailRequest : due_collection_detail
    {
    }

    public class PharmacyDueCollectionInsertRequest : due_collection_request
    {
        public PharmacyDueCollectionInsertRequest()
        {
        }

        public PharmacyDueCollectionInsertRequest(due_collection_request req)
        {
            master = req.master;
            details = req.details;
        }
    }

    public class PharmacyDueCollectionMasterData : due_collection_master
    {
    }

    public class PharmacyDueCollectionDetailData : due_collection_detail
    {
    }

    public class PharmacyDueCollectionRecord
    {
        [JsonPropertyName("master")]
        public due_collection_master? master { get; set; }

        [JsonPropertyName("details")]
        public List<due_collection_detail>? details { get; set; }
    }

    // ─── API RESPONSE WRAPPERS ──────────────────────────────────────────────────
    public class PharmacyPendingDuesApiResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public List<due_pending_bill>? Data { get; set; }
    }

    public class PharmacyBulkDueCollectionApiResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public due_bulk_collection_result? Data { get; set; }

        [JsonIgnore]
        public bool IsSuccess => string.Equals(Status, "Success", StringComparison.OrdinalIgnoreCase);
    }

    public class PharmacyDueCollectionActionResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("duecollectioncode")]
        public long? DueCollectionCode { get; set; }

        [JsonIgnore]
        public bool IsSuccess => string.Equals(Status, "Success", StringComparison.OrdinalIgnoreCase);
    }

    public class PharmacyAllDueCollectionsApiResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public List<PharmacyDueCollectionRecord>? Data { get; set; }
    }

    public class PharmacyDueCollectionByCodeApiResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public PharmacyDueCollectionRecord? Data { get; set; }
    }
}
