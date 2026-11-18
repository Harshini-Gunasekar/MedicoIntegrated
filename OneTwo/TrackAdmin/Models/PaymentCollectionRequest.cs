using System;

namespace TrackAdmin.Models
{
    public class PaymentCollectionRequest
    {
        public int RequestID { get; set; }
        public int ReferralID { get; set; }
        public string? RequestType { get; set; } // 'EmptyContainer' or 'PaymentCollection'
        public string? Status { get; set; } = "Requested"; // 'Requested', 'Accepted', 'Collected', 'ReferralConfirmed', 'Completed', 'Rejected'
        public decimal? RequestedAmount { get; set; }
        public DateTime EnteredDate { get; set; } = DateTime.UtcNow;
        
        public int? RiderID { get; set; }
        public DateTime? AcceptedTime { get; set; }
        
        public decimal? CollectedAmount { get; set; }
        public int? ContainerCount { get; set; }
        public DateTime? CollectedTime { get; set; }
        
        public decimal? ReferralConfirmedAmount { get; set; }
        public DateTime? ReferralConfirmedTime { get; set; }
        
        public bool? IsReceivedByLab { get; set; }
        public int? AdminApprovedBy { get; set; }
        public DateTime? AdminActionTime { get; set; }
        public string? AdminNotes { get; set; }
        
        public string? NotifiedRiders { get; set; }
        public string? PaymentMode { get; set; }
        
        // Discrepancy accountability properties
        public decimal? DiscrepancyAmount { get; set; }
        public decimal? ResolvedAmount { get; set; }
        public string? ResolutionType { get; set; }
        public string? ResolutionNotes { get; set; }

        // Deficit Settlement properties
        public bool? IsDeficitSettled { get; set; }
        public DateTime? DeficitSettledTime { get; set; }
        public int? DeficitSettledBy { get; set; }
        public string? DeficitSettlementNotes { get; set; }

        // Extra UI presentation properties
        public string? ReferralName { get; set; }
        public string? RiderName { get; set; }
    }
}
