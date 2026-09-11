using Dapper.Contrib.Extensions;

namespace medico_backend.Model
{
    // ═══════════════════════════════════════════════════════════════
    // OT STAFF NOTES
    // Free-flow list of medicines/tools used during a surgery, entered
    // by OT staff. Each entry only pushes to unbilledcharges if it has
    // a chargeable amount (a reusable tool with no rate stays note-only).
    // entrytype pushed: 'OT_STAFF_ITEM', entryid = note_id
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_staff_note")]
    public class OtStaffNoteModel
    {
        [ExplicitKey] public Guid note_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public string item_type { get; set; } = "MEDICINE"; // MEDICINE / TOOL / OTHER
        public string item_name { get; set; } = string.Empty;
        public decimal quantity { get; set; } = 1;
        public string? unit { get; set; }
        public decimal? rate { get; set; }
        public decimal? amount { get; set; }
        public string? used_by { get; set; }   // staff name who used/entered it

        /// <summary>Mirrors the unbilledcharges row's billedstatus for quick UI display.</summary>
        public bool is_billed { get; set; } = false;

        public string? remarks { get; set; }
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddOtStaffNoteRequest
    {
        public Guid request_id { get; set; }
        public string item_type { get; set; } = "MEDICINE";
        public string item_name { get; set; } = string.Empty;
        public decimal quantity { get; set; } = 1;
        public string? unit { get; set; }
        public decimal? rate { get; set; }
        public decimal? amount { get; set; }
        public string? used_by { get; set; }
        public string? remarks { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // OT FEE SPLIT
    // MASTER: tenant-wide reusable list of fee categories
    // (e.g. "Surgeon Fee", "Assistant Surgeon Fee", "Anesthetist Fee").
    // Same shape/pattern as ot_checklist_master.
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_fee_split_master")]
    public class OtFeeSplitMasterModel
    {
        [ExplicitKey] public Guid fee_type_id { get; set; } = Guid.NewGuid();
        public string fee_type_label { get; set; } = string.Empty;
        public bool is_active { get; set; } = true;
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddOtFeeSplitMasterRequest
    {
        public string fee_type_label { get; set; } = string.Empty;
        public bool is_active { get; set; } = true;
    }

    public class UpdateOtFeeSplitMasterRequest
    {
        public Guid fee_type_id { get; set; }
        public string fee_type_label { get; set; } = string.Empty;
        public bool is_active { get; set; } = true;
    }

    // TRANSACTION: per-request line items. Freely add as many as needed
    // (e.g. two assistant surgeons under the same fee type) — this is a
    // list like ot_consumable, NOT a one-per-type upsert like checklist items.
    // entrytype pushed: 'OT_FEE_SPLIT', entryid = item_id
    [Table("ot_fee_split_item")]
    public class OtFeeSplitItemModel
    {
        [ExplicitKey] public Guid item_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public Guid fee_type_id { get; set; }
        public string staff_name { get; set; } = string.Empty;
        public int? dcode { get; set; }        // optional link to doctor_master
        public decimal amount { get; set; }

        /// <summary>Mirrors the unbilledcharges row's billedstatus for quick UI display.</summary>
        public bool is_billed { get; set; } = false;

        public string? remarks { get; set; }
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        // populated only in GET response (joined from master), not persisted
        public string? fee_type_label { get; set; }
    }

    public class AddOtFeeSplitItemRequest
    {
        public Guid request_id { get; set; }
        public Guid fee_type_id { get; set; }
        public string staff_name { get; set; } = string.Empty;
        public int? dcode { get; set; }
        public decimal amount { get; set; }
        public string? remarks { get; set; }
    }

    // Reconciliation helper: total of all fee-split items vs. the single
    // procedure_fee on the surgery request, so the UI can flag a mismatch
    // before the case goes to billing.
    public class OtFeeSplitSummaryModel
    {
        public Guid request_id { get; set; }
        public decimal? procedure_fee { get; set; }
        public decimal total_split_amount { get; set; }
        public decimal variance { get; set; } // procedure_fee - total_split_amount
        public List<OtFeeSplitItemModel> items { get; set; } = new();
    }
}