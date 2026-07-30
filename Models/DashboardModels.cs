using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Booking.Models
{
    public class DashboardFullResponse
    {
        [JsonPropertyName("today_snapshot")]
        public TodaySnapshotModel TodaySnapshot { get; set; } = new();

        [JsonPropertyName("doctor_wise_last_7_days")]
        public List<DoctorWiseModel> DoctorWiseLast7Days { get; set; } = new();

        [JsonPropertyName("group_wise_last_7_days")]
        public List<GroupWiseModel> GroupWiseLast7Days { get; set; } = new();

        [JsonPropertyName("hourly_distribution_today")]
        public List<HourlyDistributionModel> HourlyDistributionToday { get; set; } = new();

        [JsonPropertyName("hourly_distribution_all_time")]
        public List<HourlyDistributionModel> HourlyDistributionAllTime { get; set; } = new();

        [JsonPropertyName("past_days_trend")]
        public List<PastDaysTrendModel> PastDaysTrend { get; set; } = new();

        [JsonPropertyName("investigation_breakdown_trend")]
        public List<InvestigationBreakdownModel> InvestigationBreakdownTrend { get; set; } = new();

        [JsonPropertyName("approx_turnaround_time")]
        public List<TurnaroundTimeModel> ApproxTurnaroundTime { get; set; } = new();

        [JsonPropertyName("status_funnel_today")]
        public StatusFunnelModel StatusFunnelToday { get; set; } = new();
    }

    public class TodaySnapshotModel
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("total_visits")]
        public int TotalVisits { get; set; }

        [JsonPropertyName("doctors_active")]
        public int DoctorsActive { get; set; }

        [JsonPropertyName("groups_active")]
        public int GroupsActive { get; set; }

        [JsonPropertyName("by_investigation_type")]
        public List<InvestigationSnapshotModel> ByInvestigationType { get; set; } = new();
    }

    public class InvestigationSnapshotModel
    {
        [JsonPropertyName("investigation_type")]
        public string InvestigationType { get; set; } = string.Empty;

        [JsonPropertyName("pending")]
        public int Pending { get; set; }

        [JsonPropertyName("completed")]
        public int Completed { get; set; }
    }

    public class DoctorWiseModel
    {
        [JsonPropertyName("doctor_code")]
        public int DoctorCode { get; set; }

        [JsonPropertyName("doctor_name")]
        public string DoctorName { get; set; } = string.Empty;

        [JsonPropertyName("token_count")]
        public int TokenCount { get; set; }

        [JsonPropertyName("today_count")]
        public int TodayCount { get; set; }
    }

    public class GroupWiseModel
    {
        [JsonPropertyName("group_id")]
        public int GroupId { get; set; }

        [JsonPropertyName("group_name")]
        public string GroupName { get; set; } = string.Empty;

        [JsonPropertyName("token_count")]
        public int TokenCount { get; set; }

        [JsonPropertyName("doctor_count")]
        public int DoctorCount { get; set; }

        [JsonPropertyName("today_count")]
        public int TodayCount { get; set; }
    }

    public class HourlyDistributionModel
    {
        [JsonPropertyName("hour_of_day_utc")]
        public int HourOfDayUtc { get; set; }

        [JsonPropertyName("token_count")]
        public int TokenCount { get; set; }
    }

    public class PastDaysTrendModel
    {
        [JsonPropertyName("day")]
        public string Day { get; set; } = string.Empty;

        [JsonPropertyName("token_count")]
        public int TokenCount { get; set; }

        [JsonPropertyName("waiting_for_doctor")]
        public int WaitingForDoctor { get; set; }

        [JsonPropertyName("completed")]
        public int Completed { get; set; }
    }

    public class InvestigationBreakdownModel
    {
        [JsonPropertyName("day")]
        public string Day { get; set; } = string.Empty;

        [JsonPropertyName("investigation_type")]
        public string InvestigationType { get; set; } = string.Empty;

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    public class TurnaroundTimeModel
    {
        [JsonPropertyName("investigation_type")]
        public string InvestigationType { get; set; } = string.Empty;

        [JsonPropertyName("sample_size")]
        public int SampleSize { get; set; }

        [JsonPropertyName("avg_minutes")]
        public double AvgMinutes { get; set; }

        [JsonPropertyName("min_minutes")]
        public double MinMinutes { get; set; }

        [JsonPropertyName("max_minutes")]
        public double MaxMinutes { get; set; }
    }

    public class StatusFunnelModel
    {
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("vitals_status")]
        public List<StatusCountModel> VitalsStatus { get; set; } = new();

        [JsonPropertyName("queue_status")]
        public List<StatusCountModel> QueueStatus { get; set; } = new();
    }

    public class StatusCountModel
    {
        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
