using Dapper.Contrib.Extensions;

namespace medico_backend.Model
{
    // ═══════════════════════════════════════
    // 1. IO PARTICULARS MASTER
    // ═══════════════════════════════════════
    [Table("io_particulars_master")]
    public class IoParticularsMasterModel
    {
        [Key] public int particular_id { get; set; }
        public string particular_name { get; set; } = string.Empty;
        public string? io_type { get; set; }        // INPUT / OUTPUT / BOTH
        public bool is_active { get; set; } = true;
        public string? tenant_code { get; set; }
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddIoParticularRequest
    {
        public string particular_name { get; set; } = string.Empty;
        public string? io_type { get; set; }
        public string? tenant_code { get; set; }
    }

    public class UpdateIoParticularRequest
    {
        public int particular_id { get; set; }
        public string particular_name { get; set; } = string.Empty;
        public string? io_type { get; set; }
        public bool is_active { get; set; } = true;
        public string? tenant_code { get; set; }
    }

    // ═══════════════════════════════════════
    // 2. SERVICE NAME MASTER
    // ═══════════════════════════════════════
    [Table("service_name_master")]
    public class ServiceNameMasterModel
    {
        [Key] public int service_id { get; set; }
        public string service_name { get; set; } = string.Empty;
        public bool is_active { get; set; } = true;
        public string? tenant_code { get; set; }
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddServiceNameRequest
    {
        public string service_name { get; set; } = string.Empty;
        public string? tenant_code { get; set; }
    }

    public class UpdateServiceNameRequest
    {
        public int service_id { get; set; }
        public string service_name { get; set; } = string.Empty;
        public bool is_active { get; set; } = true;
        public string? tenant_code { get; set; }
    }

    // ═══════════════════════════════════════
    // 3. SCHEDULE TYPE MASTER
    // ═══════════════════════════════════════
    [Table("schedule_type_master")]
    public class ScheduleTypeMasterModel
    {
        [Key] public int type_id { get; set; }
        public string type_name { get; set; } = string.Empty;
        public bool is_active { get; set; } = true;
        public string? tenant_code { get; set; }
        public DateTime created_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime updated_at { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class AddScheduleTypeRequest
    {
        public string type_name { get; set; } = string.Empty;
        public string? tenant_code { get; set; }
    }

    public class UpdateScheduleTypeRequest
    {
        public int type_id { get; set; }
        public string type_name { get; set; } = string.Empty;
        public bool is_active { get; set; } = true;
        public string? tenant_code { get; set; }
    }

    // ═══════════════════════════════════════
    // 4. NURSE NOTE MEDICINE
    // ═══════════════════════════════════════
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
        public decimal quantity { get; set; } = 1;
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
        public decimal quantity { get; set; } = 1;
        public string status { get; set; } = "GIVEN";
        public string? remarks { get; set; }
    }
}