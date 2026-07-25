using System;
using System.Text.Json.Serialization;

namespace Hospital_display.Models
{
    public class OgQueueModel
    {
        public int? vitalentryid { get; set; }
        
        public int? ogentryid { get; set; }

        public string? tenant_code { get; set; }

        [JsonPropertyName("token_no")]
        public string? og_token_no { get; set; }

        public string? custcode { get; set; }
        
        [JsonPropertyName("patient_name")]
        public string? custname { get; set; }

        public string? mobile { get; set; }

        public int? dcode { get; set; }

        public string? doctor_name { get; set; }

        public string? doctor_qualification { get; set; }

        public string? doctor_specialization { get; set; }

        public string? in1 { get; set; }

        public string? test_name { get; set; }

        public string? vitals_status { get; set; }

        public string? list_type { get; set; }

        private string? _status = "waiting";

        [JsonPropertyName("status")]
        public string? api_status { get; set; }

        [JsonPropertyName("queue_status")]
        public string? queue_status { get; set; }

        [JsonIgnore]
        public string? status 
        { 
            get 
            {
                if (!string.IsNullOrEmpty(api_status)) return api_status;
                if (!string.IsNullOrEmpty(queue_status)) return queue_status;
                return _status;
            }
            set 
            { 
                _status = value; 
                api_status = value;
                queue_status = value;
            }
        }

        public int? token_sort { get; set; }

        public string? out_time { get; set; }

        public string? notes { get; set; }

        public string? entered_date { get; set; }

        [JsonPropertyName("arrival_time")]
        public string? arrival_time_str { get; set; }

        public string entry_type { get; set; } = "direct"; // direct | test_completed | vitals

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime created_at { get; set; }

        public DateTime updated_at { get; set; }

        public bool deleted { get; set; } = false;

        // Custom display helpers to map to UI structure
        public string DisplayName => !string.IsNullOrWhiteSpace(custname) ? custname : (!string.IsNullOrWhiteSpace(custcode) ? custcode : "Patient");
        public string DisplayDoctor => !string.IsNullOrEmpty(doctor_name) ? doctor_name : $"Doctor #{dcode ?? 1}";

        public string DisplayTime
        {
            get
            {
                if (string.IsNullOrEmpty(arrival_time_str))
                    return DateTime.Now.ToString("hh:mm tt");

                // Parse arrival_time_str (e.g. "10:45:00" or similar)
                if (TimeSpan.TryParse(arrival_time_str, out var ts))
                {
                    return DateTime.Today.Add(ts).ToString("hh:mm tt");
                }
                return arrival_time_str;
            }
        }
    }

    public class UpdateOgOutTimeRequest
    {
        public int ogentryid { get; set; }
        public TimeOnly out_time { get; set; }
        public string? status { get; set; }
        public string? notes { get; set; }
        public int usercode { get; set; } = 1;
        public int computercode { get; set; } = 1;
    }

    public class UpdateOgStatusRequest
    {
        public int ogentryid { get; set; }
        public string status { get; set; } = "";
        public int usercode { get; set; } = 1;
        public int computercode { get; set; } = 1;
    }
}
