using System;
                                                                                                                using System.Text.Json.Serialization;
using Dapper.Contrib.Extensions;

namespace Booking.Models
{
    public class OPRegistrationModel
    {
        [Table("op_registration")]
        public class OpRegistrationModel
        {
            [ExplicitKey]
            public Guid op_id { get; set; } = Guid.NewGuid();
            public string op_no { get; set; } = string.Empty;
            public Guid? booking_id { get; set; }
            public string? booking_no { get; set; }
            public Guid? slot_detail_id { get; set; }   // ✅ NEW — links to slot
            public decimal custid { get; set; }
            public int dcode { get; set; }
            public int? department_code { get; set; }
            public string visit_type { get; set; } = "NEWVISIT";
            public string reg_type { get; set; } = "ONLINE";    // Use ONLINE token allocation for slot-driven registrations; backend walk-in ranges are not always configured.
            public DateOnly visit_date { get; set; }
            public string? token_no { get; set; }
            public int? queue_no { get; set; }
            public string visit_status { get; set; } = "WAITING";
            public string? notes { get; set; }
            public string? tenant_code { get; set; }

            [JsonConverter(typeof(FlexibleBoolConverter))]
            public bool isdeleted { get; set; } = false;

            public DateTime created_at { get; set; } =
                DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public DateTime updated_at { get; set; } =
                DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);

            [JsonConverter(typeof(FlexibleBoolConverter))]
            public bool is_direct_walkin { get; set; } = false;

            public int? duty_dcode { get; set; }
            public int? transferred_to_dcode { get; set; }
            public string? transfer_reason { get; set; }

            [JsonConverter(typeof(FlexibleBoolConverter))]
            public bool is_dressing { get; set; } = false;

            public int? service_id { get; set; }
            public int? serviceid { get; set; }

            [Write(false)]
            public string? service_name { get; set; }

            [Write(false)]
            public string? patient_name { get; set; }

            [Write(false)]
            public string? mobile { get; set; }

            [Write(false)]
            [JsonConverter(typeof(FlexibleNullableBoolConverter))]
            public bool? isvip { get; set; }

            [Write(false)]
            [JsonConverter(typeof(FlexibleNullableBoolConverter))]
            public bool? is_vip { get; set; }

            [Write(false)]
            public string? viprole { get; set; }

            [Write(false)]
            [JsonConverter(typeof(FlexibleBoolConverter))]
            public bool refer_to_ip { get; set; } = false;

            [Write(false)]
            public string? doctor_name { get; set; }

            [Write(false)]
            public string? billed_status { get; set; }

            [Write(false)]
            [JsonConverter(typeof(FlexibleNullableBoolConverter))]
            public bool? unbilled_status { get; set; }

            [Write(false)]
            [JsonConverter(typeof(FlexibleNullableBoolConverter))]
            public bool? paid_status { get; set; }

            [Write(false)]
            public TimeOnly? slot_start_time { get; set; }

            [Write(false)]
            public TimeOnly? slot_end_time { get; set; }
        }

        [Table("patient_vitals")]
        public class PatientVitalsModel
        {
            [ExplicitKey]
            public Guid vital_id { get; set; } = Guid.NewGuid();
            public Guid? op_id { get; set; }          
            public string? op_no { get; set; }        
            public Guid? ip_id { get; set; }          
            public string? ip_no { get; set; }
            public decimal custid { get; set; }
            public int dcode { get; set; }

            // ── Basic Vitals ─────────────────────────────
            public decimal? height_cm { get; set; }
            public decimal? weight_kg { get; set; }
            public decimal? bmi { get; set; }               // auto calculated
            public decimal? temperature_f { get; set; }
            public int? pulse_rate { get; set; }
            public int? respiratory_rate { get; set; }
            public int? bp_systolic { get; set; }
            public int? bp_diastolic { get; set; }
            public decimal? spo2 { get; set; }

            // ── Additional Measurements ───────────────────
            public decimal? sugar_level { get; set; }
            public int? pain_scale { get; set; }
            public decimal? waist_cm { get; set; }          
            public decimal? hip_cm { get; set; }            

            // ── Clinical Examination ──────────────────────
            public string? pedal_oedema { get; set; }       
            public string? jvp { get; set; }               
            public string? cvs { get; set; }               
            public string? rs { get; set; }                 
            public string? cns { get; set; }                
            public string? abdomen { get; set; }            

            // ── Investigations ────────────────────────────
            public string? cardiac_monitor { get; set; }    
            public string? cd_echo { get; set; }            
            public string? blood_chemistry { get; set; }    
            public string? allergy_notes { get; set; }

            // ── Special Dept ──────────────────────────────
            public decimal? hba1c { get; set; }
            public string? ecg_notes { get; set; }
            public decimal? head_circumference_cm { get; set; }

            public string? entered_by { get; set; }
            public string? tenant_code { get; set; }
            public bool isdeleted { get; set; } = false;
            public DateTime created_at { get; set; } =
                DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
            public DateTime updated_at { get; set; } =
                DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Utc);
        }

        public class UpdateVisitStatusRequest
        {
            public Guid op_id { get; set; }
            public string visit_status { get; set; } = string.Empty;
        }
        // Direct walk-in — no booking needed
        public class DirectWalkinRequest
        {
            public decimal custid { get; set; }
            public int? dcode { get; set; }           // null if patient doesn't know which doctor
            public int? duty_dcode { get; set; }      // assigned at reception if no dcode
            public int? department_code { get; set; }
            public Guid? slot_detail_id { get; set; }
            public string visit_type { get; set; } = "NEWVISIT";
            public string? notes { get; set; }
            public int? serviceid { get; set; }
            public int? service_id { get; set; }
        }

        // Transfer to another doctor after duty doctor consultation
        public class TransferDoctorRequest
        {
            public Guid op_id { get; set; }
            public int transfer_to_dcode { get; set; }
            public string? transfer_reason { get; set; }
            public Guid? slot_detail_id { get; set; }
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

            public string? notes { get; set; }
            [JsonConverter(typeof(FlexibleBoolConverter))]
            public bool refer_to_ip { get; set; } = false;
        }
        public class DressingRegistrationRequest
        {
            public decimal custid { get; set; }
            public int dcode { get; set; }
            public int? department_code { get; set; }
            public Guid? slot_detail_id { get; set; }
            public string? notes { get; set; }
        }
        public class CancelOpRegistrationRequest
        {
            public Guid op_id { get; set; }
            public string? cancel_reason { get; set; }
        }
    } 
}
