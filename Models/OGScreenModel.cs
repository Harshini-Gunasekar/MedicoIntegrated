// Model/OgQueueModel.cs
using Dapper.Contrib.Extensions;

namespace Medico_Backend.Model
{
    [Table("og_queue")]
    public class OgQueueModel
    {
        [Key]
        public int? ogentryid { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped] public string? room_no { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped] public int? group_id { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped] public string? group_name { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped] public string? custname { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped] public string? arrival_time_str { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped] public string? doctor_name { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped] public string? doctor_qualification { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped] public string? doctor_specialization { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped] public string? vitals_status { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped] public string? list_type { get; set; }
        [System.ComponentModel.DataAnnotations.Schema.NotMapped] public int? token_sort { get; set; }

        public string DisplayName => !string.IsNullOrWhiteSpace(custname) ? custname : (!string.IsNullOrWhiteSpace(custcode) ? custcode : "Patient");
        public string DisplayDoctor => !string.IsNullOrEmpty(doctor_name) ? doctor_name : $"Doctor #{dcode ?? 1}";

        public string DisplayTime
        {
            get
            {
                if (string.IsNullOrEmpty(arrival_time_str))
                    return DateTime.Now.ToString("hh:mm tt");

                if (TimeSpan.TryParse(arrival_time_str, out var ts))
                {
                    return DateTime.Today.Add(ts).ToString("hh:mm tt");
                }
                return arrival_time_str;
            }
        }

        public string? tenant_code { get; set; }

        // same token_no as vitals_entry — not generated independently anymore
        public string? og_token_no { get; set; }

        public string? custcode { get; set; }

        public int? dcode { get; set; }

        public TimeOnly? arrival_time { get; set; }

        public string entry_type { get; set; } = "direct"; // direct | test_completed

        public TimeOnly? out_time { get; set; }

        public string? notes { get; set; }

        // waiting | in_consultation | completed
        public string? status { get; set; } = "waiting";

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime created_at { get; set; }

        public DateTime updated_at { get; set; }

        public bool deleted { get; set; } = false;
    }

    public class UpdateOgOutTimeRequest
    {
        public int? ogentryid { get; set; }
        public int? vitalentryid { get; set; }
        public TimeOnly out_time { get; set; }
        public string? status { get; set; }
        public string? queue_status { get; set; }
        public string? notes { get; set; }
        public int usercode { get; set; } = 1;
        public int computercode { get; set; } = 1;
    }

    public class UpdateOgStatusRequest
    {
        public int? ogentryid { get; set; }
        public int? vitalentryid { get; set; }
        public string status { get; set; } = "";
        public string? queue_status { get; set; }
        public int usercode { get; set; } = 1;
        public int computercode { get; set; } = 1;
    }
}
