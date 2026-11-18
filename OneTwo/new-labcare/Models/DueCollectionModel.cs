namespace LabCare.Models
{
    public class DueCollectionModel
    {

    }
    // ════════════════════════════════════════════════════════════════
    //  FILTER  — request DTO
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Filter request for all 4 Due Collection tabs.
    /// Send with empty / null optional fields to load ALL pending bills on screen open.
    /// </summary>
    public class DueCollectionFilterDto
    {
        /// <summary>Tab selector: bill | patient | referral | monthly (default: bill)</summary>
        public string? type { get; set; }

        /// <summary>Branch filter. null = all branches, value = specific branch.</summary>
        public int? bhcode { get; set; }

        /// <summary>Counter filter.</summary>
        public long? cntcode { get; set; }

        // ── BILL TAB ─────────────────────────────────────────────
        public string? sampleid { get; set; }
        public string? billno { get; set; }
        public int? year { get; set; }

        // ── PATIENT TAB ──────────────────────────────────────────
        /// <summary>Name, mobile number, or numeric custid.</summary>
        public string? patient { get; set; }

        // ── REFERRAL TAB ─────────────────────────────────────────
        public long? dcode { get; set; }
        public string? referral_name { get; set; }

        // ── MONTHLY TAB ──────────────────────────────────────────
        public int? month { get; set; }
        public int? monthly_year { get; set; }

        // ── COMMON ───────────────────────────────────────────────
        public DateTime? fromdate { get; set; }
        public DateTime? todate { get; set; }

        // ── RECEIPT SEARCH ────────────────────────────────────────
        public string? receipt_no { get; set; }
        public DateTime? receipt_date { get; set; }

        // ── PAID-AMOUNT SEARCH ────────────────────────────────────
        /// <summary>
        /// When set, only bills with paidamount >= this value are returned.
        /// Those bills are sorted to the top of the result set.
        /// </summary>
        public double? paid_amount_min { get; set; }
        public double? paid_amount_max { get; set; }

        // ── PAGINATION ────────────────────────────────────────────
        public int page { get; set; } = 1;
        public int pagesize { get; set; } = 10;

    }

    // ════════════════════════════════════════════════════════════════
    //  FILTER  — response DTOs
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Paginated result wrapper returned by POST api/DueCollection/filter.
    /// </summary>
    public class DueCollectionResultDto
    {
        public string? type { get; set; }
        public int? bhcode { get; set; }

        // ── Aggregate totals (footer bar) ─────────────────────────
        public int bill_count { get; set; }
        public double total_amount { get; set; }

        /// <summary>Total pending balance (red footer bar). = totalamount - paidamount - discountamount</summary>
        public double total_balance { get; set; }

        public double total_previously_collected { get; set; }

        /// <summary>Total concession given across all matching bills (from discountamount).</summary>
        public double total_concession { get; set; }

        // ── Pagination ────────────────────────────────────────────
        public int totalcount { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
        public int totalpage { get; set; }

        // ── Rows ──────────────────────────────────────────────────
        public List<DueCollectionRowDto> data { get; set; } = new();
    }

    /// <summary>
    /// Single bill row in the due collection grid.
    /// </summary>
    public class DueCollectionRowDto
    {
        public Guid requestguid { get; set; }
        public string? sampleid { get; set; }
        public string? billno { get; set; }
        public DateTimeOffset? requestdatetime { get; set; }
        public long? custid { get; set; }
        public string? patientname { get; set; }
        public string? mobileno { get; set; }
        public long? dcode { get; set; }
        public string? referredby { get; set; }

        /// <summary>Original bill total (lrm.totalamount).</summary>
        public double balance { get; set; }

        /// <summary>
        /// Remaining balance after paid amount AND concession (discountamount).
        /// = totalamount - paidamount - discountamount
        /// </summary>
        public double curr_balance { get; set; }

        /// <summary>Amount already paid (lrm.paidamount).</summary>
        public double previously_collected { get; set; }

        /// <summary>Concession already given (lrm.discountamount).</summary>
        public double concession_given { get; set; }

        public int? enteredbhcode { get; set; }
        public string? branch_name { get; set; }
    }

    // ════════════════════════════════════════════════════════════════
    //  SAVE  — request DTOs
    // ════════════════════════════════════════════════════════════════

    /// <summary>
    /// Batch save request. All items processed in one atomic transaction.
    /// </summary>
    public class SaveDueCollectionDto
    {
        /// <summary>Payment mode: CASH | CARD | UPI etc.</summary>
        public string? collection_type { get; set; }

        public int? pmcode { get; set; }
        public int? bhcode { get; set; }
        public int? computercode { get; set; }

        /// <summary>Set by the controller from JWT — do not send from client.</summary>
        public int usercode { get; set; }
        public string? bankname { get; set; }
        public DateTime? chequedate { get; set; }
        public string? chequeno { get; set; }

        public List<SaveDueCollectionItemDto>? items { get; set; }
    }

    /// <summary>
    /// Single bill entry in a batch save.
    /// - collected_amount > 0  → receipt + payment written.
    /// - concession_amount > 0 → discountamount updated on lab_request_master (no receipt).
    /// - Both can be combined; concession is applied first.
    /// - Both = 0 → item is skipped silently.
    /// </summary>
    public class SaveDueCollectionItemDto
    {
        public Guid requestguid { get; set; }
        public double collected_amount { get; set; }

        /// <summary>
        /// Written to lab_request_master.discountamount (additive).
        /// No receipt is created.
        /// </summary>
        public double concession_amount { get; set; }
    }

    // ════════════════════════════════════════════════════════════════
    //  SAVE  — response DTOs
    // ════════════════════════════════════════════════════════════════

    public class DueCollectionSaveResponseDto
    {
        public double total_collected { get; set; }
        public double total_concession { get; set; }
        public int items_processed { get; set; }
        public DateTime collected_on { get; set; }
        public List<DueCollectionSaveResultDto> results { get; set; } = new();
    }

    /// <summary>
    /// Per-item result after save.
    /// status: "collected" | "concession_only" | "skipped"
    /// </summary>
    public class DueCollectionSaveResultDto
    {
        public Guid requestguid { get; set; }
        public string? sampleid { get; set; }
        public string? patientname { get; set; }

        /// <summary>collected | concession_only | skipped</summary>
        public string status { get; set; } = "skipped";

        public double collected { get; set; }
        public double concession { get; set; }
        public double remaining_balance { get; set; }
        public bool issettled { get; set; }

        public Guid? receiptguid { get; set; }
        public string? receiptbarcode { get; set; }
    }

    public class CollectionViewFilterDto
    {
        public DateTime? fromdate { get; set; }
        public DateTime? todate { get; set; }
        public long? cntcode { get; set; }
        public string? receipt_no { get; set; }
        public string? patient_name { get; set; }
        public string? customer { get; set; }
        public string? doctor { get; set; }
        public int? ricode { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? bhcode { get; set; }

    }

    public class DueCollectionHistoryResponseDto
    {
        public int totalcount { get; set; }
        public int page { get; set; }
        public int pagesize { get; set; }
        public int totalpage { get; set; }

        public double total_balance { get; set; }
        public double collected_amt { get; set; }

        public List<DueCollectionHistoryRowDto> data { get; set; } = new();
    }

    public class DueCollectionHistoryRowDto
    {
        public Guid receiptguid { get; set; }
        public DateTime? receiptdate { get; set; }
        public string? receipt_no { get; set; }
        public string? custcode { get; set; }
        public string? customer { get; set; }
        public string? doctor { get; set; }
        public string? monthly { get; set; }
        public string? billno { get; set; }
        public DateTime? billdate { get; set; }
        public double received { get; set; }
        public string? paymode { get; set; }
        public string? cheque_no { get; set; }
        public DateTime? cheque_date { get; set; }
        public string? bank_name { get; set; }
    }


}