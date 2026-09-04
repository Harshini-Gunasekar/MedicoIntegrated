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
    }

    public class UpdateIoParticularRequest
    {
        public int particular_id { get; set; }
        public string particular_name { get; set; } = string.Empty;
        public string? io_type { get; set; }
        public bool is_active { get; set; } = true;
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
    }

    public class UpdateServiceNameRequest
    {
        public int service_id { get; set; }
        public string service_name { get; set; } = string.Empty;
        public bool is_active { get; set; } = true;
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
    }

    public class UpdateScheduleTypeRequest
    {
        public int type_id { get; set; }
        public string type_name { get; set; } = string.Empty;
        public bool is_active { get; set; } = true;
    }
}