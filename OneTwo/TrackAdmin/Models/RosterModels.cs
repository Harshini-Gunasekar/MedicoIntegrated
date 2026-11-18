using System;

namespace TrackAdmin.Models
{
    public class RosterTimeSlot
    {
        public int SlotId { get; set; }
        public string SlotName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class RosterDutyType
    {
        public int DutyTypeId { get; set; }
        public string TypeName { get; set; }
        public string ColorHex { get; set; }
    }

    public class RosterAssignment
    {
        public Guid AssignmentId { get; set; }
        public int RiderId { get; set; }
        public int SlotId { get; set; }
        public int DutyTypeId { get; set; }
        public DateTime DutyDate { get; set; }
        public string? Zone { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class RosterAssignmentDTO
    {
        public Guid AssignmentId { get; set; }
        public int RiderId { get; set; }
        public string RiderName { get; set; }
        public int SlotId { get; set; }
        public string SlotName { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DutyTypeId { get; set; }
        public string TypeName { get; set; }
        public string ColorHex { get; set; }
        public DateTime DutyDate { get; set; }
        public string? Zone { get; set; }
        public bool IsActive { get; set; }
    }

    public class ShiftSwapRequest
    {
        public int RiderIdA { get; set; }
        public int RiderIdB { get; set; }
        public DateTime Date { get; set; }
    }

    public class ShiftCopyRequest
    {
        public int SourceRiderId { get; set; }
        public int TargetRiderId { get; set; }
        public DateTime SourceWeekStart { get; set; }
        public DateTime TargetWeekStart { get; set; }
    }

    public class BulkCopyRequest
    {
        public string Mode { get; set; } // "week" or "month"
        public DateTime SourceStart { get; set; }
        public DateTime TargetStart { get; set; }
    }

    public class RosterBulkAssignmentRequest
    {
        public List<int> RiderIds { get; set; }
        public List<DateTime> Dates { get; set; }
        public int SlotId { get; set; }
        public int DutyTypeId { get; set; }
        public string? Zone { get; set; }
    }

    public class RosterBulkClearRequest
    {
        public List<int> RiderIds { get; set; }
        public List<DateTime> Dates { get; set; }
    }
}
