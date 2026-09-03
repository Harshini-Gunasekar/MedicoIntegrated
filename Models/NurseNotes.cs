using Dapper.Contrib.Extensions;

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
            public DateTime entry_date { get; set; } = DateTime.UtcNow.Date;
            public TimeSpan entry_time { get; set; } = DateTime.UtcNow.TimeOfDay;
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
            public string shift { get; set; } = "MORNING";
            public decimal temperature { get; set; }
            public string unit { get; set; } = "F";
            public string? remarks { get; set; }
        }

        public class UpdateTemperatureRequest
        {
            public Guid temp_id { get; set; }
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
            public DateTime entry_date { get; set; } = DateTime.UtcNow.Date;
            public TimeSpan entry_time { get; set; } = DateTime.UtcNow.TimeOfDay;
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
            public DateTime entry_date { get; set; } = DateTime.UtcNow.Date;
            public TimeSpan entry_time { get; set; } = DateTime.UtcNow.TimeOfDay;
            public string shift { get; set; } = "MORNING";
            public string service_name { get; set; } = string.Empty;
            public string? action { get; set; }
            public string? remarks { get; set; }
            public int? usercode { get; set; }
            public string? tenant_code { get; set; }
            public bool isdeleted { get; set; } = false;
            public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }

        public class AddServiceRequest
        {
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public string shift { get; set; } = "MORNING";
            public string service_name { get; set; } = string.Empty;
            public string? action { get; set; }
            public string? remarks { get; set; }
        }

        public class UpdateServiceRequest
        {
            public Guid service_id { get; set; }
            public string shift { get; set; } = "MORNING";
            public string service_name { get; set; } = string.Empty;
            public string? action { get; set; }
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
            public DateTime entry_date { get; set; } = DateTime.UtcNow.Date;
            public TimeSpan entry_time { get; set; } = DateTime.UtcNow.TimeOfDay;
            public string shift { get; set; } = "MORNING";
            public string medicine_name { get; set; } = string.Empty;
            public decimal? dose { get; set; }
            public string? unit { get; set; }
            public string? route { get; set; }
            public string status { get; set; } = "GIVEN";   // GIVEN / REFUSED / MISSED
            public string? remarks { get; set; }
            public int? usercode { get; set; }
            public string? tenant_code { get; set; }
            public bool isdeleted { get; set; } = false;
            public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }

        public class AddMedicineRequest
        {
            public Guid ip_id { get; set; }
            public decimal custid { get; set; }
            public string shift { get; set; } = "MORNING";
            public string medicine_name { get; set; } = string.Empty;
            public decimal? dose { get; set; }
            public string? unit { get; set; }
            public string? route { get; set; }
            public string status { get; set; } = "GIVEN";
            public string? remarks { get; set; }
        }

        public class UpdateMedicineRequest
        {
            public Guid med_id { get; set; }
            public string shift { get; set; } = "MORNING";
            public string medicine_name { get; set; } = string.Empty;
            public decimal? dose { get; set; }
            public string? unit { get; set; }
            public string? route { get; set; }
            public string status { get; set; } = "GIVEN";
            public string? remarks { get; set; }
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
            public DateTime entry_date { get; set; } = DateTime.UtcNow.Date;
            public TimeSpan entry_time { get; set; } = DateTime.UtcNow.TimeOfDay;
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
            public string shift { get; set; } = "MORNING";
            public string note { get; set; } = string.Empty;
        }

        public class UpdateOtherRequest
        {
            public Guid other_id { get; set; }
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
            public List<NurseVisitingDoctorModel> visiting_doctor { get; set; } = new();   // NEW
            public List<NurseScheduleModel> schedule { get; set; } = new();               // NEW
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
            public DateTime entry_date { get; set; } = DateTime.UtcNow.Date;
            public TimeSpan entry_time { get; set; } = DateTime.UtcNow.TimeOfDay;
            public string shift { get; set; } = "MORNING";
            public string doctor_name { get; set; } = string.Empty;
            public string? specialization { get; set; }
            public string? visit_time { get; set; }
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
            public string shift { get; set; } = "MORNING";
            public string doctor_name { get; set; } = string.Empty;
            public string? specialization { get; set; }
            public string? visit_time { get; set; }
            public string? diagnosis_notes { get; set; }
            public string? advice { get; set; }
            public string? remarks { get; set; }
        }

        public class UpdateVisitingDoctorRequest
        {
            public Guid visit_id { get; set; }
            public string shift { get; set; } = "MORNING";
            public string doctor_name { get; set; } = string.Empty;
            public string? specialization { get; set; }
            public string? visit_time { get; set; }
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
            public DateTime entry_date { get; set; } = DateTime.UtcNow.Date;
            public TimeSpan entry_time { get; set; } = DateTime.UtcNow.TimeOfDay;
            public string shift { get; set; } = "MORNING";
            public string scheduled_date { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
            public string scheduled_time { get; set; } = "08:00:00";
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
            public string shift { get; set; } = "MORNING";
            public string scheduled_date { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
            public string scheduled_time { get; set; } = "08:00:00";
            public string item_type { get; set; } = "MEDICINE";
            public string item_name { get; set; } = string.Empty;
            public string? instructions { get; set; }
            public string status { get; set; } = "PENDING";
            public string? remarks { get; set; }
        }

        public class UpdateScheduleRequest
        {
            public Guid schedule_id { get; set; }
            public string shift { get; set; } = "MORNING";
            public string scheduled_date { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");
            public string scheduled_time { get; set; } = "08:00:00";
            public string item_type { get; set; } = "MEDICINE";
            public string item_name { get; set; } = string.Empty;
            public string? instructions { get; set; }
            public string status { get; set; } = "PENDING";
            public string? remarks { get; set; }
        }
    }
}