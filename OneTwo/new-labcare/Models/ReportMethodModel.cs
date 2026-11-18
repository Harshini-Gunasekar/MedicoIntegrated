using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace LabCare.Models
{
    [Table("report_method")]
    public class ReportMethodModel
    {
        [Key]
        [JsonProperty("rtmcode")]
        public long rtmcode { get; set; }

        [JsonProperty("orderno")]
        public int orderno { get; set; }

        [JsonProperty("shortname")]
        public string? shortname { get; set; }

        [JsonProperty("name")]
        public string? name { get; set; }

        [JsonProperty("durationtime")]
        public int durationtime { get; set; }

        [JsonProperty("duration")]
        public string? duration { get; set; }

        [JsonProperty("description")]
        public string? description { get; set; }

        [JsonProperty("footer")]
        public string? footer { get; set; }

        [JsonProperty("isculture")]
        public bool isculture { get; set; }

        [JsonProperty("deleted")]
        public bool deleted { get; set; } = false;

        [JsonProperty("usercode")]
        public int usercode { get; set; } = 1;

        [JsonProperty("computercode")]
        public int computercode { get; set; } = 1;

        [JsonProperty("entereddate")]
        public DateTime entereddate { get; set; }

        [JsonProperty("ibsdate")]
        public DateTime ibsdate { get; set; }

        [JsonProperty("tenant_code")]
        public string? tenant_code { get; set; }
    }
}
