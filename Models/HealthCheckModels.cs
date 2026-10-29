using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Booking.Models
{
    public class HealthCheckResponse
    {
        [JsonPropertyName("app")]
        public string App { get; set; } = "Medico Web";

        [JsonPropertyName("status")]
        public string Status { get; set; } = "healthy";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("checkedAtUtc")]
        public string CheckedAtUtc { get; set; } = DateTime.UtcNow.ToString("o");

        [JsonPropertyName("apis")]
        public List<ApiHealthDetail> Apis { get; set; } = new();
    }

    public class ApiHealthDetail
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = "healthy";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "";

        [JsonPropertyName("responseTimeMs")]
        public long ResponseTimeMs { get; set; }
    }
}

namespace yourapp.Models
{
    public class HealthCheckResponse : Booking.Models.HealthCheckResponse { }
    public class ApiHealthDetail : Booking.Models.ApiHealthDetail { }
    public class AppSettings : SharedComponents.Rcl.Models.AppSettings { }
}
