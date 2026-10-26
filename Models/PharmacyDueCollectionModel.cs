using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Booking.Models
{
    // =========================================================
    //  PHARMACY DUE COLLECTION — MODELS & DTOS
    // =========================================================

    /// <summary>
    /// Pending due bill item from GET /api/ItemMaster/getpendingdues
    /// </summary>
    public class PharmacyPendingDueBill
    {
        [JsonPropertyName("salescode")]
        public long salescode { get; set; }

        [JsonPropertyName("billno")]
        public string? billno { get; set; }

        [JsonPropertyName("billdate")]
        public DateTime? billdate { get; set; }

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

    /// <summary>
    /// API Response wrapper for getpendingdues
    /// </summary>
    public class PharmacyPendingDuesApiResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("data")]
        public List<PharmacyPendingDueBill>? Data { get; set; }
    }

    /// <summary>
    /// Insert / Update Due Collection Request payload
    /// </summary>
    public class PharmacyDueCollectionInsertRequest
    {
        [JsonPropertyName("master")]
        public PharmacyDueCollectionMasterRequest master { get; set; } = new();

        [JsonPropertyName("details")]
        public List<PharmacyDueCollectionDetailRequest> details { get; set; } = new();
    }

    public class PharmacyDueCollectionMasterRequest
    {
        [JsonPropertyName("duecollectioncode")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? duecollectioncode { get; set; }

        [JsonPropertyName("requestdate")]
        public DateTime requestdate { get; set; } = DateTime.Now;

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

        [JsonPropertyName("paymentmode")]
        public string? paymentmode { get; set; } = "CASH";

        [JsonPropertyName("chequeno")]
        public string? chequeno { get; set; }

        [JsonPropertyName("accountno")]
        public string? accountno { get; set; }

        [JsonPropertyName("referenceno")]
        public string? referenceno { get; set; }

        [JsonPropertyName("remarks")]
        public string? remarks { get; set; }

        [JsonPropertyName("usercode")]
        public int? usercode { get; set; }

        [JsonPropertyName("branchcode")]
        public string? branchcode { get; set; }

        [JsonPropertyName("companycode")]
        public string? companycode { get; set; }

        [JsonPropertyName("bh_code")]
        public int? bh_code { get; set; }

        [JsonPropertyName("cntcode")]
        public int? cntcode { get; set; }

        [JsonPropertyName("cnttid")]
        public string? cnttid { get; set; }
    }

    public class PharmacyDueCollectionDetailRequest
    {
        [JsonPropertyName("salescode")]
        public long salescode { get; set; }

        [JsonPropertyName("collectedamount")]
        public decimal collectedamount { get; set; }

        [JsonPropertyName("remarks")]
        public string? remarks { get; set; }
    }

    /// <summary>
    /// Insert / Update / Delete Action response
    /// </summary>
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

    /// <summary>
    /// Existing Due Collection Record with Master & Details
    /// </summary>
    public class PharmacyDueCollectionRecord
    {
        [JsonPropertyName("master")]
        public PharmacyDueCollectionMasterData? master { get; set; }

        [JsonPropertyName("details")]
        public List<PharmacyDueCollectionDetailData>? details { get; set; }
    }

    public class PharmacyDueCollectionMasterData
    {
        [JsonPropertyName("duecollectioncode")]
        public long duecollectioncode { get; set; }

        [JsonPropertyName("requestno")]
        public string? requestno { get; set; }

        [JsonPropertyName("requestdate")]
        public DateTime? requestdate { get; set; }

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
        public bool isactive { get; set; }

        [JsonPropertyName("deleted")]
        public bool deleted { get; set; }

        [JsonPropertyName("createddate")]
        public DateTime? createddate { get; set; }

        [JsonPropertyName("modifieddate")]
        public DateTime? modifieddate { get; set; }

        [JsonPropertyName("usercode")]
        public int? usercode { get; set; }

        [JsonPropertyName("tenantcode")]
        public string? tenantcode { get; set; }

        [JsonPropertyName("branchcode")]
        public string? branchcode { get; set; }

        [JsonPropertyName("companycode")]
        public string? companycode { get; set; }

        [JsonPropertyName("bh_code")]
        public int? bh_code { get; set; }

        [JsonPropertyName("cntcode")]
        public int? cntcode { get; set; }

        [JsonPropertyName("cnttid")]
        public string? cnttid { get; set; }
    }

    public class PharmacyDueCollectionDetailData
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
