using System;
using System.Linq;
using Dapper.Contrib.Extensions;

namespace Medico_Backend.Model
{
    [Table("vitals_entry")]
    public class VitalsModel
    {
        [Key]
        public int vitalentryid { get; set; }

        public string? tenant_code { get; set; }

        public string? token_no { get; set; }

        public string? custcode { get; set; }

        [Write(false)]
        public string? patient_name { get; set; }

        public int? dcode { get; set; }

        // REPLACED "investigation" (single string) with 5 slots
        public string? in1 { get; set; }
        public string? in2 { get; set; }
        public string? in3 { get; set; }
        public string? in4 { get; set; }
        public string? in5 { get; set; }

        // Computed helper property for UI display and two-way binding
        [Write(false)]
        public string? investigation
        {
            get => string.Join(", ", new[] { in1, in2, in3, in4, in5 }.Where(s => !string.IsNullOrWhiteSpace(s)));
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    in1 = in2 = in3 = in4 = in5 = null;
                }
                else
                {
                    in1 = value;
                }
            }
        }

        public string? test_name { get; set; }

        public string? status { get; set; } = "waiting_for_test";

        // NEW — true = VIP, gets reserved dummy-slot token (1, 26, 51, 76, 101...)
        public bool is_vip { get; set; } = false;

        public DateTime entered_date { get; set; }

        public TimeOnly? arrival_time { get; set; }

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime created_at { get; set; }

        public DateTime updated_at { get; set; }

        public bool deleted { get; set; } = false;
    }

    public class UpdateVitalStatusRequest
    {
        public int vitalentryid { get; set; }
        public string? status { get; set; }
        public int usercode { get; set; } = 1;
        public int computercode { get; set; } = 1;
    }

    public class LabResultEntryModel
    {
        public int vitalentryid { get; set; }
        public string? token_no { get; set; }
        public string? custcode { get; set; }
        public string? patient_name { get; set; }
        public string? mobile { get; set; }
        public string? test_name { get; set; }
        public string? status { get; set; }
        public DateTime entered_date { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class LabStatusUpdateRequest
    {
        public int vitalentryid { get; set; }
        public string status { get; set; } = "";   // waiting_for_test | on_going | completed | result_pending | report_received
        public int usercode { get; set; } = 1;
        public int computercode { get; set; } = 1;
    }

    public class ScanResultEntryModel
    {
        public int vitalentryid { get; set; }
        public string? token_no { get; set; }
        public string? custcode { get; set; }
        public string? patient_name { get; set; }
        public string? mobile { get; set; }
        public string? test_name { get; set; }   // scan test, e.g. Ultrasound, X-Ray
        public string? status { get; set; }
        public DateTime entered_date { get; set; }
        public DateTime updated_at { get; set; }
    }

    public class ScanStatusUpdateRequest
    {
        public int vitalentryid { get; set; }
        public string status { get; set; } = "";
        public int usercode { get; set; } = 1;
        public int computercode { get; set; } = 1;
    }
}
