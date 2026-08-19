using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Booking.Models
{
    [Table("appointment_booking")]
    public class AppointmentBookingModel
    {
        [Key]
        public Guid booking_id { get; set; } = Guid.NewGuid();
        public string? booking_no { get; set; }
        public decimal custid { get; set; }
        public int dcode { get; set; }
        public Guid slot_detail_id { get; set; }
        public Guid slot_master_id { get; set; }
        public DateOnly appointment_date { get; set; }
        public TimeOnly slot_start_time { get; set; }
        public TimeOnly slot_end_time { get; set; }
        public int token_no { get; set; } = 0;
        public string booking_status { get; set; } = "BOOKED";
        public string booking_type { get; set; } = "ONLINE";
        public Guid? rescheduled_from { get; set; }
        public string? reschedule_reason { get; set; }
        public string? cancel_reason { get; set; }
        public DateTime? cancelled_at { get; set; }
        public string? notes { get; set; }
        public string? tenant_code { get; set; }
        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool isdeleted { get; set; } = false;
        public DateTime created_at { get; set; } =
            DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        public DateTime updated_at { get; set; } =
            DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

        public string? patient_name { get; set; }
        public string? mobile { get; set; }

        [JsonConverter(typeof(FlexibleNullableBoolConverter))]
        public bool? isvip { get; set; }

        [JsonConverter(typeof(FlexibleNullableBoolConverter))]
        public bool? is_vip { get; set; }

        public string? viprole { get; set; }

        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool is_dressing { get; set; } = false;

        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool refer_to_ip { get; set; } = false;
    }

    public class CancelAppointmentRequest
    {
        public Guid booking_id { get; set; }
        public string cancel_reason { get; set; } = string.Empty;
    }

    public class RescheduleAppointmentRequest
    {
        public Guid old_booking_id { get; set; }
        public string booking_type { get; set; } = "ONLINE";
        public string? reschedule_reason { get; set; }
        public AppointmentBookingModel new_booking { get; set; } = new();
    }
    
    public class RescheduleSlotItemRequest
    {
        public Guid old_booking_id { get; set; }
        public string booking_type { get; set; } = "ONLINE";
        public string? reschedule_reason { get; set; }
        public Guid new_slot_detail_id { get; set; }
        public Guid new_slot_master_id { get; set; }
        public DateOnly new_appointment_date { get; set; }
        public TimeOnly new_slot_start_time { get; set; }
        public TimeOnly new_slot_end_time { get; set; }
        public int new_dcode { get; set; }
        public string? notes { get; set; }
    }

    public class RescheduleWholeSlotRequest
    {
        public Guid slot_master_id { get; set; }
        public Guid new_slot_detail_id { get; set; }
        public Guid new_slot_master_id { get; set; }
        public DateOnly new_appointment_date { get; set; }
        public TimeOnly new_slot_start_time { get; set; }
        public TimeOnly new_slot_end_time { get; set; }
        public int new_dcode { get; set; }
        public string? reschedule_reason { get; set; }
    }
    
    public class AppointmentBookingViewModel
    {
        public Guid booking_id { get; set; }
        public decimal custid { get; set; }
        public string? customer_name { get; set; }
        public string? mobile { get; set; }
        public int dcode { get; set; }
        public DateOnly appointment_date { get; set; }
        public TimeOnly slot_start_time { get; set; }
        public TimeOnly slot_end_time { get; set; }
        public int token_no { get; set; }
        public string? booking_status { get; set; }
        public string? booking_type { get; set; }
        public string? tenant_code { get; set; }
    }
    
    public class AppointmentBookingLogModel
    {
        public Guid log_id { get; set; } = Guid.NewGuid();
        public Guid booking_id { get; set; }
        public string? booking_no { get; set; }
        public decimal custid { get; set; }
        public int dcode { get; set; }
        public string action { get; set; } = string.Empty;
        public string? action_by { get; set; }
        public Guid? old_slot_detail_id { get; set; }
        public Guid? new_slot_detail_id { get; set; }
        public DateOnly? old_appointment_date { get; set; }
        public DateOnly? new_appointment_date { get; set; }
        public TimeOnly? old_slot_start_time { get; set; }
        public TimeOnly? new_slot_start_time { get; set; }
        public string? remarks { get; set; }
        public string? tenant_code { get; set; }
        public DateTime created_at { get; set; } =
            DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
    }

    public class PatientRescheduleRequest
    {
        public Guid old_booking_id { get; set; }
        public string booking_type { get; set; } = "ONLINE";
        public string? reschedule_reason { get; set; }
        public AppointmentBookingModel new_booking { get; set; } = new();
    }
    
    public class AvailableSlotModel
    {
        public Guid slot_detail_id { get; set; }
        public Guid slot_master_id { get; set; }
        public int dcode { get; set; }
        public string? typeofslot { get; set; }
        public DateOnly appointment_date { get; set; }
        public TimeOnly slot_start_time { get; set; }
        public TimeOnly slot_end_time { get; set; }
        public int max_patients { get; set; }
        public int max_walkin { get; set; }
        public int max_online { get; set; }
        public int patient_count { get; set; }
        public int walkin_count { get; set; }
        public int online_count { get; set; }
        public int booked_count { get; set; }
        public int remaining_seats { get; set; }
        public int remaining_walkin { get; set; }
        public int remaining_online { get; set; }
    }

    public class DoctorBookingListModel
    {
        public Guid booking_id { get; set; }
        public string? booking_no { get; set; }
        public decimal custid { get; set; }
        public string? patient_name { get; set; }
        public int dcode { get; set; }
        public DateOnly appointment_date { get; set; }
        public TimeOnly slot_start_time { get; set; }
        public TimeOnly slot_end_time { get; set; }
        public int token_no { get; set; }
        public string? booking_status { get; set; }
        public string? booking_type { get; set; }
        public string? reg_type { get; set; }
        public string? notes { get; set; }
        [JsonConverter(typeof(FlexibleNullableBoolConverter))]
        public bool? isvip { get; set; }
        public string? viprole { get; set; }
        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool refer_to_ip { get; set; } = false;

        public Guid op_id { get; set; }

        [JsonConverter(typeof(FlexibleNullableBoolConverter))]
        public bool? paid_status { get; set; }

        [JsonConverter(typeof(FlexibleNullableBoolConverter))]
        public bool? unbilled_status { get; set; }

        [JsonConverter(typeof(FlexibleBoolConverter))]
        public bool is_dressing { get; set; } = false;

        public string? display_token_no { get; set; }
    }
}
