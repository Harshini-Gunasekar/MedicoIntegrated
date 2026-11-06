using Dapper.Contrib.Extensions;
using System.Text.Json.Serialization;

namespace medico_backend.Model
{
    public class IpBedsideChartModel
    {
        // ─────────────────────────────────────────
        // 1. TEMPERATURE
        // ─────────────────────────────────────────
        [Table("ip_nurse_note_temperature")]
        public class NurseTemperatureModel
        {
            [ExplicitKey] public Guid temp_id { get; set; } = Guid.NewGuid();
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public DateOnly entry_date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
            public TimeOnly entry_time { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);
            public string shift { get; set; } = "MORNING";
            public decimal temperature { get; set; }
            public string unit { get; set; } = "F";
            public string? remarks { get; set; }
            public int? usercode { get; set; }
            public string? tenant_code { get; set; }
            public bool isdeleted { get; set; } = false;
            public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }

        public class AddTemperatureRequest
        {
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public int usercode { get; set; }                       // CHANGED — now read from body
            public DateOnly? entry_date { get; set; }               // NEW — optional, server time if null
            public TimeOnly? entry_time { get; set; }               // NEW — optional, server time if null
            public string shift { get; set; } = "MORNING";
            public decimal temperature { get; set; }
            public string unit { get; set; } = "F";
            public string? remarks { get; set; }
        }

        public class UpdateTemperatureRequest
        {
            public Guid temp_id { get; set; }
            public DateOnly? entry_date { get; set; }               // NEW — optional, unchanged if null
            public TimeOnly? entry_time { get; set; }               // NEW — optional, unchanged if null
            public string shift { get; set; } = "MORNING";
            public decimal temperature { get; set; }
            public string unit { get; set; } = "F";
            public string? remarks { get; set; }
        }

        // ─────────────────────────────────────────
        // 2. VITALS — NOT modeled here. Reuses the existing
        // vitals table via api/OpRegistration/save-vitals,
        // update-vitals, vitals/all, vitals/detail (pass ip_id).
        // ─────────────────────────────────────────

        // ─────────────────────────────────────────
        // 3. INPUT / OUTPUT
        // ─────────────────────────────────────────
        [Table("ip_nurse_note_input_output")]
        public class NurseInputOutputModel
        {
            [ExplicitKey] public Guid io_id { get; set; } = Guid.NewGuid();
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public DateOnly entry_date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
            public TimeOnly entry_time { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);
            public string shift { get; set; } = "MORNING";
            public string io_type { get; set; } = "INPUT";   // INPUT / OUTPUT
            public string particulars { get; set; } = string.Empty;
            public decimal? quantity { get; set; }
            public string unit { get; set; } = "ml";
            public string? remarks { get; set; }
            public int? usercode { get; set; }
            public string? tenant_code { get; set; }
            public bool isdeleted { get; set; } = false;
            public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }

        public class AddInputOutputRequest
        {
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public int usercode { get; set; }                       // CHANGED — now read from body
            public DateOnly? entry_date { get; set; }               // NEW
            public TimeOnly? entry_time { get; set; }               // NEW
            public string shift { get; set; } = "MORNING";
            public string io_type { get; set; } = "INPUT";
            public string particulars { get; set; } = string.Empty;
            public decimal? quantity { get; set; }
            public string unit { get; set; } = "ml";
            public string? remarks { get; set; }
        }

        public class UpdateInputOutputRequest
        {
            public Guid io_id { get; set; }
            public DateOnly? entry_date { get; set; }               // NEW
            public TimeOnly? entry_time { get; set; }               // NEW
            public string shift { get; set; } = "MORNING";
            public string io_type { get; set; } = "INPUT";
            public string particulars { get; set; } = string.Empty;
            public decimal? quantity { get; set; }
            public string unit { get; set; } = "ml";
            public string? remarks { get; set; }
        }

        // ─────────────────────────────────────────
        // 4. SERVICE
        // ─────────────────────────────────────────
        [Table("ip_nurse_note_service")]
        public class NurseServiceModel
        {
            [ExplicitKey] public Guid service_id { get; set; } = Guid.NewGuid();
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public DateOnly entry_date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
            public TimeOnly entry_time { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);
            public string shift { get; set; } = "MORNING";
            public string service_name { get; set; } = string.Empty;
            public string? action { get; set; }
            public string? remarks { get; set; }
            public int? usercode { get; set; }
            public string? tenant_code { get; set; }
            public bool isdeleted { get; set; } = false;
            public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            // Attached charge info from unbilledcharges (not columns on ip_nurse_note_service)
            [Write(false)] public decimal? quantity { get; set; }
            [Write(false)] public decimal? rate { get; set; }
            [Write(false)] public decimal? amount { get; set; }
            [Write(false)] public bool? billedstatus { get; set; }
            [Write(false)] public string? billno { get; set; }
        }

        public class AddServiceRequest
        {
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public int usercode { get; set; }                       // CHANGED — now read from body
            public DateOnly? entry_date { get; set; }               // NEW
            public TimeOnly? entry_time { get; set; }               // NEW
            public string shift { get; set; } = "MORNING";
            public string service_name { get; set; } = string.Empty;   // resolved against test_master
            public string? action { get; set; }
            public decimal quantity { get; set; } = 1;                  // used to compute the unbilled amount
            public decimal? rate { get; set; }                          // optional custom/override rate
            public string? remarks { get; set; }
        }

        public class UpdateServiceRequest
        {
            public Guid service_id { get; set; }
            public DateOnly? entry_date { get; set; }               // NEW
            public TimeOnly? entry_time { get; set; }               // NEW
            public string shift { get; set; } = "MORNING";
            public string service_name { get; set; } = string.Empty;
            public string? action { get; set; }
            public decimal? quantity { get; set; }                  // NEW — used to re-price the fee (defaults to 1)
            public decimal? rate { get; set; }                          // optional custom/override rate
            public string? remarks { get; set; }
        }

        // ─────────────────────────────────────────
        // 5. MEDICINE
        // ─────────────────────────────────────────
        [Table("ip_nurse_note_medicine")]
        public class NurseMedicineModel
        {
            [ExplicitKey] public Guid med_id { get; set; } = Guid.NewGuid();
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public DateOnly entry_date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
            public TimeOnly entry_time { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);
            public string shift { get; set; } = "MORNING";
            public string medicine_name { get; set; } = string.Empty;
            public decimal? dose { get; set; }
            public string? unit { get; set; }
            public string? route { get; set; }
            public decimal quantity { get; set; } = 1;
            public string status { get; set; } = "GIVEN";   // GIVEN / REFUSED / MISSED
            public string? remarks { get; set; }
            public int? usercode { get; set; }
            public string? tenant_code { get; set; }
            public bool isdeleted { get; set; } = false;
            public bool sent_to_pharmacy { get; set; } = false;
            public DateTime? sent_to_pharmacy_at { get; set; }
            public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

            // CaseSheet-style prescription fields
            public int? sno { get; set; }
            public string? pr_code { get; set; }
            public decimal? drug_code { get; set; }
            public string? generic_name { get; set; }
            public string? drug_category { get; set; }
            public string morning { get; set; } = "0";
            public string afternoon { get; set; } = "0";
            public string evening { get; set; } = "0";
            public string night { get; set; } = "0";
            public string? morning_dose { get; set; }
            public string? afternoon_dose { get; set; }
            public string? evening_dose { get; set; }
            public string? night_dose { get; set; }
            public string? dose_unit { get; set; }
            public bool before_food { get; set; } = false;
            public bool after_food { get; set; } = false;
            public int? days { get; set; }
            public decimal? rate { get; set; }
            public decimal? mrp { get; set; }
            public string? batch_no { get; set; }
            public string? batchno { get => batch_no; set => batch_no = value; }
            public DateTime? batch_expiry_date { get; set; }
            public long? stockcode { get; set; }
            public long? warehousecode { get; set; }
            public string? notes { get; set; }
        }

        public class AddMedicineRequest
        {
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public int usercode { get; set; }                       // CHANGED — now read from body
            public DateOnly? entry_date { get; set; }               // NEW
            public TimeOnly? entry_time { get; set; }               // NEW
            public string shift { get; set; } = "MORNING";
            public string medicine_name { get; set; } = string.Empty;
            public decimal? dose { get; set; }
            public string? unit { get; set; }
            public string? route { get; set; }
            public decimal quantity { get; set; } = 1;
            public decimal? qty { get; set; }
            public string status { get; set; } = "GIVEN";
            public string? remarks { get; set; }

            // CaseSheet-style prescription fields
            public int? sno { get; set; }
            public string? pr_code { get; set; }
            public decimal? drug_code { get; set; }
            public string? generic_name { get; set; }
            public string? drug_category { get; set; }
            public string morning { get; set; } = "0";
            public string afternoon { get; set; } = "0";
            public string evening { get; set; } = "0";
            public string night { get; set; } = "0";
            public string? morning_dose { get; set; }
            public string? afternoon_dose { get; set; }
            public string? evening_dose { get; set; }
            public string? night_dose { get; set; }
            public string? dose_unit { get; set; }
            public bool before_food { get; set; } = false;
            public bool after_food { get; set; } = false;
            public int? days { get; set; }
            public decimal? rate { get; set; }
            public decimal? mrp { get; set; }
            public string? batch_no { get; set; }
            public string? batchno { get => batch_no; set => batch_no = value; }
            public DateOnly? batch_expiry_date { get; set; }        // DateOnly — backend expects "YYYY-MM-DD" format
            public long? stockcode { get; set; }
            public long? warehousecode { get; set; }
            public string? notes { get; set; }
        }

        public class UpdateMedicineRequest
        {
            public Guid med_id { get; set; }
            public DateOnly? entry_date { get; set; }               // NEW
            public TimeOnly? entry_time { get; set; }               // NEW
            public string shift { get; set; } = "MORNING";
            public string medicine_name { get; set; } = string.Empty;
            public decimal? dose { get; set; }
            public string? unit { get; set; }
            public string? route { get; set; }
            public decimal quantity { get; set; } = 1;
            public decimal? qty { get; set; }
            public string status { get; set; } = "GIVEN";
            public string? remarks { get; set; }

            // CaseSheet-style prescription fields
            public int? sno { get; set; }
            public string? pr_code { get; set; }
            public decimal? drug_code { get; set; }
            public string? generic_name { get; set; }
            public string? drug_category { get; set; }
            public string morning { get; set; } = "0";
            public string afternoon { get; set; } = "0";
            public string evening { get; set; } = "0";
            public string night { get; set; } = "0";
            public string? morning_dose { get; set; }
            public string? afternoon_dose { get; set; }
            public string? evening_dose { get; set; }
            public string? night_dose { get; set; }
            public string? dose_unit { get; set; }
            public bool before_food { get; set; } = false;
            public bool after_food { get; set; } = false;
            public int? days { get; set; }
            public decimal? rate { get; set; }
            public decimal? mrp { get; set; }
            public string? batch_no { get; set; }
            public string? batchno { get => batch_no; set => batch_no = value; }
            public DateOnly? batch_expiry_date { get; set; }        // DateOnly — backend expects "YYYY-MM-DD" format
            public long? stockcode { get; set; }
            public long? warehousecode { get; set; }
            public string? notes { get; set; }
        }

        public class SaveNursePrescriptionRequest
        {
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public int usercode { get; set; }
            public bool is_authorized { get; set; } = true;
            public DateOnly? entry_date { get; set; }
            public TimeOnly? entry_time { get; set; }
            public string shift { get; set; } = "MORNING";
            public string? pr_code { get; set; }
            public string? topremarks { get; set; }
            public string? bottonremarks { get; set; }
            public List<CaseSheetPrescriptionItem> items { get; set; } = new();
        }

        // ─────────────────────────────────────────
        // 6. OTHER
        // ─────────────────────────────────────────
        [Table("ip_nurse_note_other")]
        public class NurseOtherModel
        {
            [ExplicitKey] public Guid other_id { get; set; } = Guid.NewGuid();
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public DateOnly entry_date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
            public TimeOnly entry_time { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);
            public string shift { get; set; } = "MORNING";
            public string note { get; set; } = string.Empty;
            public int? usercode { get; set; }
            public string? tenant_code { get; set; }
            public bool isdeleted { get; set; } = false;
            public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }

        public class AddOtherRequest
        {
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public int usercode { get; set; }                       // CHANGED — now read from body
            public DateOnly? entry_date { get; set; }               // NEW
            public TimeOnly? entry_time { get; set; }               // NEW
            public string shift { get; set; } = "MORNING";
            public string note { get; set; } = string.Empty;
        }

        public class UpdateOtherRequest
        {
            public Guid other_id { get; set; }
            public DateOnly? entry_date { get; set; }               // NEW
            public TimeOnly? entry_time { get; set; }               // NEW
            public string shift { get; set; } = "MORNING";
            public string note { get; set; } = string.Empty;
        }

        // ─────────────────────────────────────────
        // COMMON — used by delete/update calls
        // ─────────────────────────────────────────
        public class NurseNoteIdRequest
        {
            public Guid id { get; set; }
        }

        // Combined view for the "SELECTED TAB CONTENT" screen — one call
        // returns everything this module owns. Vitals and OP Case Sheet
        // are NOT included — fetch those separately from their existing
        // endpoints (api/OpRegistration/vitals/all, api/CaseSheet/by-visit)
        // since they live in tables owned by other modules.
        public class NurseNotesSummaryModel
        {
            public List<NurseTemperatureModel> temperature { get; set; } = new();
            public List<NurseInputOutputModel> input_output { get; set; } = new();
            public List<NurseServiceModel> service { get; set; } = new();
            public List<NurseMedicineModel> medicine { get; set; } = new();
            public List<NurseOtherModel> other { get; set; } = new();
            public List<NurseVisitingDoctorModel> visiting_doctor { get; set; } = new();
            public List<NurseScheduleModel> schedule { get; set; } = new();
            public List<NurseDiabeticModel> diabetic { get; set; } = new();
        }

        // ─────────────────────────────────────────
        // 7. VISITING DOCTOR LIST
        // ─────────────────────────────────────────
        [Table("ip_nurse_note_visiting_doctor")]
        public class NurseVisitingDoctorModel
        {
            [ExplicitKey] public Guid visit_id { get; set; } = Guid.NewGuid();
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public DateOnly entry_date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
            public TimeOnly entry_time { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);
            public string shift { get; set; } = "MORNING";
            public string doctor_name { get; set; } = string.Empty;
            public string? specialization { get; set; }
            public TimeOnly? visit_time { get; set; }
            public string? diagnosis_notes { get; set; }
            public string? advice { get; set; }
            public string? remarks { get; set; }
            public int? usercode { get; set; }
            public string? tenant_code { get; set; }
            public bool isdeleted { get; set; } = false;
            public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }

        public class AddVisitingDoctorRequest
        {
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public int usercode { get; set; }                       // CHANGED — now read from body
            public DateOnly? entry_date { get; set; }               // NEW
            public TimeOnly? entry_time { get; set; }               // NEW
            public string shift { get; set; } = "MORNING";
            public string doctor_name { get; set; } = string.Empty;
            public string? specialization { get; set; }
            public TimeOnly? visit_time { get; set; }
            public string? diagnosis_notes { get; set; }
            public string? advice { get; set; }
            public string? remarks { get; set; }
        }

        public class UpdateVisitingDoctorRequest
        {
            public Guid visit_id { get; set; }
            public DateOnly? entry_date { get; set; }               // NEW
            public TimeOnly? entry_time { get; set; }               // NEW
            public string shift { get; set; } = "MORNING";
            public string doctor_name { get; set; } = string.Empty;
            public string? specialization { get; set; }
            public TimeOnly? visit_time { get; set; }
            public string? diagnosis_notes { get; set; }
            public string? advice { get; set; }
            public string? remarks { get; set; }
        }

        // ─────────────────────────────────────────
        // 8. SCHEDULE
        // ─────────────────────────────────────────
        [Table("ip_nurse_note_schedule")]
        public class NurseScheduleModel
        {
            [ExplicitKey] public Guid schedule_id { get; set; } = Guid.NewGuid();
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public DateOnly entry_date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
            public TimeOnly entry_time { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);
            public string shift { get; set; } = "MORNING";
            public DateOnly scheduled_date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
            public TimeOnly scheduled_time { get; set; }
            public string item_type { get; set; } = "MEDICINE";   // MEDICINE / DIET / PROCEDURE / OTHER
            public string item_name { get; set; } = string.Empty;
            public string? instructions { get; set; }
            public string status { get; set; } = "PENDING";       // PENDING / COMPLETED / MISSED
            public string? remarks { get; set; }
            public int? usercode { get; set; }
            public string? tenant_code { get; set; }
            public bool isdeleted { get; set; } = false;
            public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }

        public class AddScheduleRequest
        {
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public int usercode { get; set; }                       // CHANGED — now read from body
            public DateOnly? entry_date { get; set; }               // NEW
            public TimeOnly? entry_time { get; set; }               // NEW
            public string shift { get; set; } = "MORNING";
            public DateOnly scheduled_date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
            public TimeOnly scheduled_time { get; set; }
            public string item_type { get; set; } = "MEDICINE";
            public string item_name { get; set; } = string.Empty;
            public string? instructions { get; set; }
            public string status { get; set; } = "PENDING";
            public string? remarks { get; set; }
        }

        public class UpdateScheduleRequest
        {
            public Guid schedule_id { get; set; }
            public DateOnly? entry_date { get; set; }               // NEW
            public TimeOnly? entry_time { get; set; }               // NEW
            public string shift { get; set; } = "MORNING";
            public DateOnly scheduled_date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
            public TimeOnly scheduled_time { get; set; }
            public string item_type { get; set; } = "MEDICINE";
            public string item_name { get; set; } = string.Empty;
            public string? instructions { get; set; }
            public string status { get; set; } = "PENDING";
            public string? remarks { get; set; }
        }

        // ─────────────────────────────────────────
        // 9. AUTHORIZATION (single authorize button for the whole chart)
        // ─────────────────────────────────────────
        [Table("ip_nurse_note_authorization")]
        public class NurseNoteAuthorizationModel
        {
            [ExplicitKey] public Guid auth_id { get; set; } = Guid.NewGuid();
            public Guid ip_id { get; set; }
            public decimal? custid { get; set; }
            public bool is_authorized { get; set; }
            public int? authorized_by { get; set; }
            public DateTime? authorized_at { get; set; }
            public int? updated_by { get; set; }
            public DateTime? updated_at { get; set; }
            public string tenant_code { get; set; } = string.Empty;

            [Write(false)]
            [JsonPropertyName("authorized_by_name")]
            public string? authorized_by_name { get; set; }

            [Write(false)]
            [JsonPropertyName("user_name")]
            public string? user_name { get; set; }

            [Write(false)]
            [JsonPropertyName("username")]
            public string? username { get; set; }

            [Write(false)]
            [JsonPropertyName("name")]
            public string? name { get; set; }

            [Write(false)]
            [JsonPropertyName("staff_name")]
            public string? staff_name { get; set; }
        }

        public class AuthorizeNurseNotesRequest
        {
            public Guid ip_id { get; set; }
            public decimal? custid { get; set; }
            public int usercode { get; set; }                       // CHANGED — now read from body
        }

        // ─────────────────────────────────────────
        // 10. DIABETIC CHART
        // ─────────────────────────────────────────
        [Table("ip_nurse_note_diabetic")]
        public class NurseDiabeticModel
        {
            [ExplicitKey] public Guid diabetic_id { get; set; } = Guid.NewGuid();
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public DateOnly entry_date { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
            public TimeOnly entry_time { get; set; } = TimeOnly.FromDateTime(DateTime.UtcNow);
            public string shift { get; set; } = "MORNING";
            public string reading_type { get; set; } = "RANDOM";
            public decimal blood_sugar { get; set; }
            public string? insulin_type { get; set; }
            public decimal? insulin_dose { get; set; }
            public string? insulin_unit { get; set; } = "units";
            public string? site { get; set; }
            public string? remarks { get; set; }
            public int? usercode { get; set; }
            public string? tenant_code { get; set; }
            public bool isdeleted { get; set; } = false;
            public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }

        public class AddDiabeticRequest
        {
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public int usercode { get; set; }
            public DateOnly? entry_date { get; set; }               // NEW
            public TimeOnly? entry_time { get; set; }               // NEW
            public string shift { get; set; } = "MORNING";
            public string reading_type { get; set; } = "RANDOM";
            public decimal blood_sugar { get; set; }
            public string? insulin_type { get; set; }
            public decimal? insulin_dose { get; set; }
            public string? insulin_unit { get; set; } = "units";
            public string? site { get; set; }
            public string? remarks { get; set; }
        }

        public class UpdateDiabeticRequest
        {
            public Guid diabetic_id { get; set; }
            public DateOnly? entry_date { get; set; }               // NEW
            public TimeOnly? entry_time { get; set; }               // NEW
            public string shift { get; set; } = "MORNING";
            public string reading_type { get; set; } = "RANDOM";
            public decimal blood_sugar { get; set; }
            public string? insulin_type { get; set; }
            public decimal? insulin_dose { get; set; }
            public string? insulin_unit { get; set; } = "units";
            public string? site { get; set; }
            public string? remarks { get; set; }
        }
    }
}
