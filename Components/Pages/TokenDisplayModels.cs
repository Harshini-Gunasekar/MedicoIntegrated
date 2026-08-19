using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Medico.Components.Pages
{
    public class FlexibleStringConverter : JsonConverter<string?>
    {
        public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
                return reader.GetString();
            if (reader.TokenType == JsonTokenType.True)
                return "true";
            if (reader.TokenType == JsonTokenType.False)
                return "false";
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            using var doc = JsonDocument.ParseValue(ref reader);
            return doc.RootElement.GetRawText();
        }

        public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
        {
            if (value == null)
                writer.WriteNullValue();
            else
                writer.WriteStringValue(value);
        }
    }

    public class FlexibleIntConverter : JsonConverter<int?>
    {
        public override int? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out int i))
                    return i;
                if (reader.TryGetDouble(out double d))
                    return (int)d;
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                string? str = reader.GetString();
                if (int.TryParse(str, out int i))
                    return i;
                if (double.TryParse(str, out double d))
                    return (int)d;
            }
            else if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }
            return null;
        }

        public override void Write(Utf8JsonWriter writer, int? value, JsonSerializerOptions options)
        {
            if (value.HasValue)
                writer.WriteNumberValue(value.Value);
            else
                writer.WriteNullValue();
        }
    }

    public class TokenOgQueueModel
    {
        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? vitalentryid { get; set; }
        
        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? ogentryid { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? tenant_code { get; set; }

        [JsonPropertyName("token_no")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? og_token_no { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? custcode { get; set; }
        
        [JsonPropertyName("patient_name")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? custname { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? mobile { get; set; }

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? dcode { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? doctor_name { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? doctor_qualification { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? doctor_specialization { get; set; }

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? group_id { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? group_name { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? room_no { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? in1 { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? test_name { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? vitals_status { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? list_type { get; set; }

        private string? _status = "waiting";

        [JsonPropertyName("status")]
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? api_status { get; set; }

        [JsonPropertyName("queue_status")]
        [JsonConverter(typeof(FlexibleStringConverter))]
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

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? token_sort { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? out_time { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? notes { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? entered_date { get; set; }

        [JsonPropertyName("arrival_time")]
        [JsonConverter(typeof(FlexibleStringConverter))]
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




    public class TokenDoctorMasterModel
    {
        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? dcode { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? name { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? nametitle { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? doctorfullname { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? qualification { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? doctorimage { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? room_no { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? token_prefix { get; set; }

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? group_id { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? tenant_code { get; set; }

        [JsonConverter(typeof(FlexibleIntConverter))]
        public int? spcode { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? doctorcode { get; set; }

        public bool deleted { get; set; } = false;

        [JsonIgnore]
        public string? DoctorImageBase64 { get; set; }

        public string GetFormattedName()
        {
            if (!string.IsNullOrWhiteSpace(doctorfullname))
                return doctorfullname;
            
            if (!string.IsNullOrWhiteSpace(name))
            {
                string title = !string.IsNullOrWhiteSpace(nametitle) ? nametitle.Trim() : "Dr.";
                if (!title.EndsWith(".")) title += ".";
                return $"{title} {name.Trim()}";
            }

            return $"Doctor #{dcode}";
        }
    }

    public class FileDownloadResponse
    {
        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? fileName { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? contentType { get; set; }

        [JsonConverter(typeof(FlexibleStringConverter))]
        public string? base64 { get; set; }
    }

    public class DoctorCurrentStatusModel
    {
        public int status_id { get; set; }
        public int dcode { get; set; }
        public string? tenant_code { get; set; }
        public string? status { get; set; }
        public string? remarks { get; set; }
        public DateTime? expected_return_time { get; set; }
    }
}
