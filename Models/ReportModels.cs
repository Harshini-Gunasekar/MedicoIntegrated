using System;
using System.Collections.Generic;

namespace Booking.Models
{
    public class ReportRevenueItem
    {
        public DateTime Date { get; set; }
        public string BillNo { get; set; } = "";
        public string PatientName { get; set; } = "";
        public string DoctorName { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal TotalAmount { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal NetAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DueAmount { get; set; }
        public string PayMode { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsIp { get; set; }
    }

    public class DailyRevenueSummary
    {
        public DateTime Date { get; set; }
        public string DateLabel { get; set; } = "";
        public decimal IpRevenue { get; set; }
        public decimal OpRevenue { get; set; }
        public decimal TotalRevenue => IpRevenue + OpRevenue;
        public int IpTransactionCount { get; set; }
        public int OpTransactionCount { get; set; }
        public int TotalTransactions => IpTransactionCount + OpTransactionCount;
    }

    public class CombinedRevenueReport
    {
        public List<ReportRevenueItem> IpItems { get; set; } = new();
        public List<ReportRevenueItem> OpItems { get; set; } = new();
        public List<DailyRevenueSummary> DailySummaries { get; set; } = new();
        
        public decimal TotalIpRevenue { get; set; }
        public decimal TotalOpRevenue { get; set; }
        public decimal GrandTotalRevenue => TotalIpRevenue + TotalOpRevenue;
        public int TotalIpCount { get; set; }
        public int TotalOpCount { get; set; }
        public int GrandTotalCount => TotalIpCount + TotalOpCount;

        public decimal TodayIpRevenue { get; set; }
        public decimal TodayOpRevenue { get; set; }
        public decimal TodayTotalRevenue => TodayIpRevenue + TodayOpRevenue;
    }
}
