using System;

namespace TrackAdmin.Models
{
    public class WelcomeStats
    {
        public RiderShiftStats RiderStats { get; set; } = new();
        public WorkStats TodayStats { get; set; } = new();
        public WorkStats YesterdayStats { get; set; } = new();
    }

    public class RiderShiftStats
    {
        public int TotalRiders { get; set; }
        public int MorningShift { get; set; }
        public int EveningShift { get; set; }
        public int NightShift { get; set; }
    }

    public class WorkStats
    {
        public int TotalAppointments { get; set; }
        public int SampleCollectionsCount { get; set; }
        public int TotalSamplesCollected { get; set; }
        public int PaymentCollectionsCount { get; set; }
        public double TotalPaymentsCollectedAmount { get; set; }
        public int PaymentRequestsRaisedCount { get; set; }
        public int PaymentRequestsAcceptedCount { get; set; }
        public int PaymentRequestsCompletedCount { get; set; }
        public int PaymentRequestsPendingCount { get; set; }
    }
}
