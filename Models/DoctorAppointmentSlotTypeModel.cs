using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Booking.Models
{
    [Table("doctor_appointment_slot_type")]
    public class DoctorAppointmentSlotTypeModel
    {
        [Key]
        public long slot_type_id { get; set; }

        public string? name { get; set; }

        public string? shortname { get; set; }

        public string? colorcode { get; set; }

        public string? description { get; set; }

        public DateTime entereddate { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        public DateTime ibsdate { get; set; } = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        public bool deleted { get; set; } = false;

        public string? tenant_code { get; set; }
    }
}
