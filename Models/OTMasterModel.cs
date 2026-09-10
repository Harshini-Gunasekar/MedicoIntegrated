using Dapper.Contrib.Extensions;

namespace medico_backend.Model
{
    // ═══════════════════════════════════════════════════════════════
    // 1. OT MASTER
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_master")]
    public class OtMasterModel
    {
        [ExplicitKey] public Guid ot_id { get; set; } = Guid.NewGuid();
        public string ot_name { get; set; } = string.Empty;
        public string? location { get; set; }
        public string? specialty { get; set; }
        public TimeOnly? timing_from { get; set; }
        public TimeOnly? timing_to { get; set; }
        public string? equipment { get; set; }
        public bool is_active { get; set; } = true;
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddOtMasterRequest
    {
        public string ot_name { get; set; } = string.Empty;
        public string? location { get; set; }
        public string? specialty { get; set; }
        public TimeOnly? timing_from { get; set; }
        public TimeOnly? timing_to { get; set; }
        public string? equipment { get; set; }
        public bool is_active { get; set; } = true;
    }

    public class UpdateOtMasterRequest
    {
        public Guid ot_id { get; set; }
        public string ot_name { get; set; } = string.Empty;
        public string? location { get; set; }
        public string? specialty { get; set; }
        public TimeOnly? timing_from { get; set; }
        public TimeOnly? timing_to { get; set; }
        public string? equipment { get; set; }
        public bool is_active { get; set; } = true;
    }

    // ═══════════════════════════════════════════════════════════════
    // 2. SURGERY REQUEST
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_surgery_request")]
    public class OtSurgeryRequestModel
    {
        [ExplicitKey] public Guid request_id { get; set; } = Guid.NewGuid();
        public string? op_id { get; set; }
        public Guid? ip_id { get; set; }
        public decimal custid { get; set; }
        public int dcode { get; set; }
        public string? diagnosis { get; set; }
        public string? surgery_type { get; set; }  // e.g. MAJOR / MINOR / DAYCARE / LOCAL
        public string procedure_name { get; set; } = string.Empty;
        public string? procedure_code { get; set; }
        public string priority { get; set; } = "ELECTIVE";
        public int? expected_duration_minutes { get; set; }
        public string? anesthesia_type { get; set; }

        /// <summary>
        /// One-time OT/surgeon usage fee. If set (> 0) when the request is
        /// created, it is pushed into unbilledcharges as entrytype
        /// 'OT_PROCEDURE' with entryid = request_id, the same way an
        /// investigation test is pushed on save.
        /// </summary>
        public decimal? procedure_fee { get; set; }

        public string? notes { get; set; }
        public string status { get; set; } = "REQUESTED";
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddSurgeryRequestRequest
    {
        public string? op_id { get; set; }
        public Guid? ip_id { get; set; }
        public decimal custid { get; set; }
        public int dcode { get; set; }
        public string? diagnosis { get; set; }
        public string? surgery_type { get; set; }  // e.g. MAJOR / MINOR / DAYCARE / LOCAL
        public string procedure_name { get; set; } = string.Empty;
        public string? procedure_code { get; set; }
        public string priority { get; set; } = "ELECTIVE";
        public int? expected_duration_minutes { get; set; }
        public string? anesthesia_type { get; set; }
        public decimal? procedure_fee { get; set; }   // NEW
        public string? notes { get; set; }
    }

    public class UpdateSurgeryRequestRequest
    {
        public Guid request_id { get; set; }
        public string? diagnosis { get; set; }
        public string? surgery_type { get; set; }  // e.g. MAJOR / MINOR / DAYCARE / LOCAL
        public string procedure_name { get; set; } = string.Empty;
        public string? procedure_code { get; set; }
        public string priority { get; set; } = "ELECTIVE";
        public int? expected_duration_minutes { get; set; }
        public string? anesthesia_type { get; set; }
        public string? notes { get; set; }
        // procedure_fee is intentionally NOT editable here — once charged,
        // correct it via the common billing screen's unbilled-charge editor
        // (UnbilledChargesClass.UpdateUnbilledCharge), same as investigations.
    }

    public class UpdateSurgeryStatusRequest
    {
        public Guid request_id { get; set; }
        public string status { get; set; } = string.Empty;
    }

    // ═══════════════════════════════════════════════════════════════
    // 3. OT SCHEDULE
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_schedule")]
    public class OtScheduleModel
    {
        [ExplicitKey] public Guid schedule_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public Guid ot_id { get; set; }
        public decimal custid { get; set; }
        public DateOnly scheduled_date { get; set; }
        public TimeOnly scheduled_time { get; set; }
        public TimeOnly estimated_end_time { get; set; }
        public string status { get; set; } = "BOOKED";
        public string? notes { get; set; }
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddOtScheduleRequest
    {
        public Guid request_id { get; set; }
        public Guid ot_id { get; set; }
        public decimal custid { get; set; }
        public DateOnly scheduled_date { get; set; }
        public TimeOnly scheduled_time { get; set; }
        public TimeOnly estimated_end_time { get; set; }
        public string? notes { get; set; }
    }

    public class UpdateOtScheduleRequest
    {
        public Guid schedule_id { get; set; }
        public Guid ot_id { get; set; }
        public DateOnly scheduled_date { get; set; }
        public TimeOnly scheduled_time { get; set; }
        public TimeOnly estimated_end_time { get; set; }
        public string status { get; set; } = "BOOKED";
        public string? notes { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // 4. OT TEAM
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_team")]
    public class OtTeamMemberModel
    {
        [ExplicitKey] public Guid team_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public string role { get; set; } = string.Empty;
        public int? dcode { get; set; }
        public string staff_name { get; set; } = string.Empty;
        public string? staff_code { get; set; }
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddOtTeamMemberRequest
    {
        public Guid request_id { get; set; }
        public string role { get; set; } = string.Empty;
        public int? dcode { get; set; }
        public string staff_name { get; set; } = string.Empty;
        public string? staff_code { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // 5. PRE-OP ASSESSMENT
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_preop_assessment")]
    public class OtPreOpAssessmentModel
    {
        [ExplicitKey] public Guid preop_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public int? assessed_by_dcode { get; set; }
        public string fitness_status { get; set; } = "PENDING";
        public string? medical_history { get; set; }
        public string? allergies { get; set; }
        public string? investigations_summary { get; set; }
        public string? vitals_summary { get; set; }
        public string? clearance_notes { get; set; }
        public DateTime assessment_date { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddPreOpAssessmentRequest
    {
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public int? assessed_by_dcode { get; set; }
        public string fitness_status { get; set; } = "PENDING";
        public string? medical_history { get; set; }
        public string? allergies { get; set; }
        public string? investigations_summary { get; set; }
        public string? vitals_summary { get; set; }
        public string? clearance_notes { get; set; }
    }

    public class UpdatePreOpAssessmentRequest
    {
        public Guid preop_id { get; set; }
        public string fitness_status { get; set; } = "PENDING";
        public string? medical_history { get; set; }
        public string? allergies { get; set; }
        public string? investigations_summary { get; set; }
        public string? vitals_summary { get; set; }
        public string? clearance_notes { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // 6. CONSENT
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_consent")]
    public class OtConsentModel
    {
        [ExplicitKey] public Guid consent_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public string consent_type { get; set; } = "SURGERY";
        public bool consent_given { get; set; } = false;
        public string? consent_by { get; set; }
        public string? relation { get; set; }
        public string? witness_name { get; set; }
        public DateTime consent_date { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public string? remarks { get; set; }
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddOtConsentRequest
    {
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public string consent_type { get; set; } = "SURGERY";
        public bool consent_given { get; set; } = false;
        public string? consent_by { get; set; }
        public string? relation { get; set; }
        public string? witness_name { get; set; }
        public string? remarks { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // 7. OT CHECKLIST
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_checklist")]
    public class OtChecklistModel
    {
        [ExplicitKey] public Guid checklist_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public bool patient_confirmed { get; set; } = false;
        public bool procedure_confirmed { get; set; } = false;
        public bool site_marked { get; set; } = false;
        public bool consent_confirmed { get; set; } = false;
        public bool investigations_confirmed { get; set; } = false;
        public bool blood_arranged { get; set; } = false;
        public bool instruments_confirmed { get; set; } = false;
        public bool implants_confirmed { get; set; } = false;
        public bool antibiotic_given { get; set; } = false;
        public string? checked_by { get; set; }
        public DateTime? checked_at { get; set; }
        public string? remarks { get; set; }
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class SaveOtChecklistRequest
    {
        public Guid request_id { get; set; }
        public bool patient_confirmed { get; set; } = false;
        public bool procedure_confirmed { get; set; } = false;
        public bool site_marked { get; set; } = false;
        public bool consent_confirmed { get; set; } = false;
        public bool investigations_confirmed { get; set; } = false;
        public bool blood_arranged { get; set; } = false;
        public bool instruments_confirmed { get; set; } = false;
        public bool implants_confirmed { get; set; } = false;
        public bool antibiotic_given { get; set; } = false;
        public string? checked_by { get; set; }
        public string? remarks { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // 8. ANESTHESIA
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_anesthesia")]
    public class OtAnesthesiaModel
    {
        [ExplicitKey] public Guid anesthesia_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public string? anesthesia_type { get; set; }
        public int? anesthetist_dcode { get; set; }
        public string? asa_grade { get; set; }
        public string? airway_assessment { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        public string? drugs_used { get; set; }
        public string? fluids_given { get; set; }
        public string? complications { get; set; }
        public string? remarks { get; set; }
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class SaveOtAnesthesiaRequest
    {
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public string? anesthesia_type { get; set; }
        public int? anesthetist_dcode { get; set; }
        public string? asa_grade { get; set; }
        public string? airway_assessment { get; set; }
        public DateTime? start_time { get; set; }
        public DateTime? end_time { get; set; }
        public string? drugs_used { get; set; }
        public string? fluids_given { get; set; }
        public string? complications { get; set; }
        public string? remarks { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // 9. INTRA-OP / SURGERY RECORD
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_intraop")]
    public class OtIntraOpModel
    {
        [ExplicitKey] public Guid intraop_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public string? procedure_performed { get; set; }
        public string? diagnosis_pre { get; set; }
        public string? diagnosis_post { get; set; }
        public string? findings { get; set; }
        public DateTime? surgery_start_time { get; set; }
        public DateTime? surgery_end_time { get; set; }
        public int? blood_loss_ml { get; set; }
        public string? complications { get; set; }
        public string? remarks { get; set; }
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class SaveOtIntraOpRequest
    {
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public string? procedure_performed { get; set; }
        public string? diagnosis_pre { get; set; }
        public string? diagnosis_post { get; set; }
        public string? findings { get; set; }
        public DateTime? surgery_start_time { get; set; }
        public DateTime? surgery_end_time { get; set; }
        public int? blood_loss_ml { get; set; }
        public string? complications { get; set; }
        public string? remarks { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // 10. CONSUMABLES
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_consumable")]
    public class OtConsumableModel
    {
        [ExplicitKey] public Guid consumable_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public string item_name { get; set; } = string.Empty;
        public string? item_code { get; set; }
        public string? batch_no { get; set; }
        public decimal quantity { get; set; } = 1;
        public string? unit { get; set; }
        public decimal? rate { get; set; }
        public decimal? amount { get; set; }

        /// <summary>Mirrors the unbilledcharges row's billedstatus for quick UI display.</summary>
        public bool is_billed { get; set; } = false;   // NEW

        public string? remarks { get; set; }
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddOtConsumableRequest
    {
        public Guid request_id { get; set; }
        public string item_name { get; set; } = string.Empty;
        public string? item_code { get; set; }
        public string? batch_no { get; set; }
        public decimal quantity { get; set; } = 1;
        public string? unit { get; set; }
        public decimal? rate { get; set; }
        public decimal? amount { get; set; }
        public string? remarks { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // 11. IMPLANTS
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_implant")]
    public class OtImplantModel
    {
        [ExplicitKey] public Guid implant_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public string item_name { get; set; } = string.Empty;
        public string? batch_no { get; set; }
        public string? serial_no { get; set; }
        public string? manufacturer { get; set; }
        public decimal quantity { get; set; } = 1;
        public decimal? rate { get; set; }
        public decimal? amount { get; set; }

        /// <summary>Mirrors the unbilledcharges row's billedstatus for quick UI display.</summary>
        public bool is_billed { get; set; } = false;   // NEW

        public string? remarks { get; set; }
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddOtImplantRequest
    {
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public string item_name { get; set; } = string.Empty;
        public string? batch_no { get; set; }
        public string? serial_no { get; set; }
        public string? manufacturer { get; set; }
        public decimal quantity { get; set; } = 1;
        public decimal? rate { get; set; }
        public decimal? amount { get; set; }
        public string? remarks { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // 12. RECOVERY / PACU
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_recovery")]
    public class OtRecoveryModel
    {
        [ExplicitKey] public Guid recovery_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public DateTime? admission_time { get; set; }
        public DateTime? discharge_time { get; set; }
        public string? vitals_summary { get; set; }
        public int? pain_score { get; set; }
        public string? oxygen_support { get; set; }
        public string recovery_status { get; set; } = "IN_RECOVERY";
        public string? transferred_to { get; set; }
        public string? remarks { get; set; }
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class SaveOtRecoveryRequest
    {
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public DateTime? admission_time { get; set; }
        public DateTime? discharge_time { get; set; }
        public string? vitals_summary { get; set; }
        public int? pain_score { get; set; }
        public string? oxygen_support { get; set; }
        public string recovery_status { get; set; } = "IN_RECOVERY";
        public string? transferred_to { get; set; }
        public string? remarks { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // 13. POST-OP ORDERS
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_postop_order")]
    public class OtPostOpOrderModel
    {
        [ExplicitKey] public Guid order_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public string order_type { get; set; } = "MEDICATION";
        public string order_text { get; set; } = string.Empty;
        public string? instructions { get; set; }
        public int? ordered_by_dcode { get; set; }
        public string status { get; set; } = "PENDING";
        public DateTime order_date { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddOtPostOpOrderRequest
    {
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public string order_type { get; set; } = "MEDICATION";
        public string order_text { get; set; } = string.Empty;
        public string? instructions { get; set; }
        public int? ordered_by_dcode { get; set; }
    }

    public class UpdateOtPostOpOrderStatusRequest
    {
        public Guid order_id { get; set; }
        public string status { get; set; } = "COMPLETED";
    }

    // ═══════════════════════════════════════════════════════════════
    // 14. OPERATION NOTE
    // ═══════════════════════════════════════════════════════════════
    [Table("ot_operation_note")]
    public class OtOperationNoteModel
    {
        [ExplicitKey] public Guid note_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public int? surgeon_dcode { get; set; }
        public string? pre_op_diagnosis { get; set; }
        public string? post_op_diagnosis { get; set; }
        public string? procedure_performed { get; set; }
        public string? findings { get; set; }
        public int? blood_loss_ml { get; set; }
        public bool specimen_sent { get; set; } = false;
        public string? specimen_details { get; set; }
        public string? complications { get; set; }
        public string? instructions { get; set; }
        public DateTime note_date { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class SaveOtOperationNoteRequest
    {
        public Guid request_id { get; set; }
        public decimal custid { get; set; }
        public int? surgeon_dcode { get; set; }
        public string? pre_op_diagnosis { get; set; }
        public string? post_op_diagnosis { get; set; }
        public string? procedure_performed { get; set; }
        public string? findings { get; set; }
        public int? blood_loss_ml { get; set; }
        public bool specimen_sent { get; set; } = false;
        public string? specimen_details { get; set; }
        public string? complications { get; set; }
        public string? instructions { get; set; }
    }

    // ═══════════════════════════════════════════════════════════════
    // 15. OT SUMMARY — read-only aggregate, no table of its own.
    // Billing state is now derived live from unbilledcharges instead
    // of a separate "OT bill" record — same pattern as investigations.
    // ═══════════════════════════════════════════════════════════════
    public class OtSummaryModel
    {
        public OtSurgeryRequestModel? request { get; set; }
        public OtScheduleModel? schedule { get; set; }
        public List<OtTeamMemberModel> team { get; set; } = new();
        public OtPreOpAssessmentModel? preop { get; set; }
        public List<OtConsentModel> consents { get; set; } = new();
        public OtChecklistModel? checklist { get; set; }
        public OtAnesthesiaModel? anesthesia { get; set; }
        public OtIntraOpModel? intraop { get; set; }
        public List<OtConsumableModel> consumables { get; set; } = new();
        public List<OtImplantModel> implants { get; set; } = new();
        public OtRecoveryModel? recovery { get; set; }
        public List<OtPostOpOrderModel> postop_orders { get; set; } = new();
        public List<OtSterilizationModel> sterilization { get; set; } = new();
        public List<OtChecklistItemModel> checklist_extra_items { get; set; } = new();
        public OtOperationNoteModel? operation_note { get; set; }

        // Derived straight from unbilledcharges — no separate bill record.
        public int total_charge_count { get; set; }
        public int pending_charge_count { get; set; }
        public bool is_fully_billed { get; set; }
    }
    [Table("ot_sterilization")]
    public class OtSterilizationModel
    {
        [ExplicitKey] public Guid sterilization_id { get; set; } = Guid.NewGuid();
        public Guid request_id { get; set; }
        public string instrument_set { get; set; } = string.Empty;
        public string sterilization_method { get; set; } = "AUTOCLAVE";
        public string? cycle_number { get; set; }
        public string? sterilized_by { get; set; }
        public DateTime sterilization_date { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime? expiry_date { get; set; }
        public string status { get; set; } = "STERILE";
        public string? remarks { get; set; }
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddOtSterilizationRequest
    {
        public Guid request_id { get; set; }
        public string instrument_set { get; set; } = string.Empty;
        public string sterilization_method { get; set; } = "AUTOCLAVE";
        public string? cycle_number { get; set; }
        public string? sterilized_by { get; set; }
        public DateTime? expiry_date { get; set; }
        public string status { get; set; } = "STERILE";
        public string? remarks { get; set; }
    }
    public class PostponeOtScheduleRequest
    {
        public Guid schedule_id { get; set; }
        public DateOnly new_scheduled_date { get; set; }
        public TimeOnly new_scheduled_time { get; set; }
        public TimeOnly new_estimated_end_time { get; set; }
        public string? reason { get; set; }
    }
    // ── MASTER ──────────────────────────────────────
    [Table("ot_checklist_master")]
    public class OtChecklistMasterModel
    {
        [ExplicitKey] public Guid item_master_id { get; set; } = Guid.NewGuid();
        public string item_label { get; set; } = string.Empty;
        public string? category { get; set; }
        public bool is_active { get; set; } = true;
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddOtChecklistMasterRequest
    {
        public string item_label { get; set; } = string.Empty;
        public string? category { get; set; }
        public bool is_active { get; set; } = true;
    }

    public class UpdateOtChecklistMasterRequest
    {
        public Guid item_master_id { get; set; }
        public string item_label { get; set; } = string.Empty;
        public string? category { get; set; }
        public bool is_active { get; set; } = true;
    }

    // ── TRANSACTION (per request, via checklist_id) ──
    [Table("ot_checklist_item")]
    public class OtChecklistItemModel
    {
        [ExplicitKey] public Guid item_id { get; set; } = Guid.NewGuid();
        public Guid checklist_id { get; set; }
        public Guid item_master_id { get; set; }
        public bool is_checked { get; set; } = false;
        public string? checked_by { get; set; }
        public DateTime? checked_at { get; set; }
        public string? remarks { get; set; }
        public string? tenant_code { get; set; }
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        // populated only in GET response, not persisted
        public string? item_label { get; set; }
        public string? category { get; set; }
    }

    // request_id is used at the API boundary (that's what the caller knows);
    // internally we resolve it to checklist_id since ot_checklist is 1-per-request.
    public class SaveOtChecklistItemRequest
    {
        public Guid request_id { get; set; }
        public Guid item_master_id { get; set; }
        public bool is_checked { get; set; } = false;
        public string? checked_by { get; set; }
        public string? remarks { get; set; }
    }
    public class OtTimeSlot
    {
        public TimeOnly start_time { get; set; }
        public TimeOnly end_time { get; set; }
    }

    public class OtDayAvailability
    {
        public DateOnly date { get; set; }
        public List<OtTimeSlot> available_slots { get; set; } = new();
        public List<OtTimeSlot> booked_slots { get; set; } = new();
    }

    public class OtAvailabilitySlotsResponse
    {
        public Guid ot_id { get; set; }
        public string ot_name { get; set; } = string.Empty;
        public DateOnly date { get; set; }
        public TimeOnly day_start { get; set; }
        public TimeOnly day_end { get; set; }
        public List<OtTimeSlot> available_slots { get; set; } = new();
        public List<OtTimeSlot> booked_slots { get; set; } = new();
    }

    public class OtAvailabilityRangeResponse
    {
        public Guid ot_id { get; set; }
        public string ot_name { get; set; } = string.Empty;
        public TimeOnly day_start { get; set; }
        public TimeOnly day_end { get; set; }
        public List<OtDayAvailability> days { get; set; } = new();
    }
    public class OtSurgeryRequestListModel : OtSurgeryRequestModel
{
    public string? patient_name { get; set; }
    public string? mobile { get; set; }
    public string? gender { get; set; }
    public int? ageyears { get; set; }
    public bool? isvip { get; set; }

    public string? doctor_name { get; set; }
    public string? doctor_qualification { get; set; }   
    public int? doctor_spcode { get; set; }              
}
}