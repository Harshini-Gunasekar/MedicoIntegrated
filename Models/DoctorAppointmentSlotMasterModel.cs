using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Models
{
    [Table("doctor_appointment_slot_master")]
    public class DoctorAppointmentSlotMasterModel
    {
    [Key]
    public Guid slot_master_id { get; set; } = Guid.NewGuid();
    public int slotnum { get; set; }
    public int dcode { get; set; }
    public long slot_type_id { get; set; }
    public string? tenant_code { get; set; }

    public int? avgtime { get; set; }

    public string? day_of_week { get; set; }
    public TimeOnly slot_start_time { get; set; }
    public TimeOnly slot_end_time { get; set; }
    public string? typeofslot { get; set; }
    public int max_patients { get; set; } = 0;
    public int max_walkin { get; set; } = 0;
    public int max_online { get; set; } = 0;

    public DateOnly? slot_date { get; set; }
    public bool is_active { get; set; } = true;
    public bool isdeleted { get; set; } = false;
    public DateTime created_at { get; set; } =DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    public DateTime updated_at { get; set; } =DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    public bool is_cancel { get; set; } = false;
    public string? cancel_reason { get; set; }
        // Helper for UI display
        [NotMapped]
        public string? DoctorName { get; set; }

         public class DoctorAppointmentSlotMasterBulkModel
         {
             public List<DoctorAppointmentSlotMasterModel> Slots { get; set; } = new();
         }
    }
}
