using System;
using Booking.Models;
using Booking.Helpers;

namespace Booking.Services
{
    public class WalkinStateService
    {
        public event Action? OnChange;

        public void NotifyStateChanged() => OnChange?.Invoke();

        public string DraftType { get; set; } = "RECEPTION"; // "RECEPTION" or "DIRECT_OP"
        public bool ShowWalkinModal { get; set; }
        public bool IsWalkinMinimized { get; set; }
        public string PatientSearchQuery { get; set; } = "";
        public decimal WalkinCustid { get; set; }
        public DateTime WalkinDate { get; set; } = DateTime.UtcNow.ToIndianTime().Date;
        public int WalkinDcode { get; set; }
        public int DutyDcode { get; set; }
        public string WalkinRegType { get; set; } = "OP";
        public int SelectedServiceId { get; set; } = 0;
        public string WalkinNotes { get; set; } = "";
        public AvailableSlotModel? SelectedWalkinSlot { get; set; }
        public Guid? SelectedSlotDetailId { get; set; }

        public bool HasActiveDraft => ShowWalkinModal;

        public string Title => DraftType == "DIRECT_OP" ? "Direct Walk-in OP" : "Walk-in Appointment";

        public string TargetPageUri => DraftType == "DIRECT_OP" ? "/booking/outpatient" : "/booking/reception";

        public void Clear()
        {
            DraftType = "RECEPTION";
            ShowWalkinModal = false;
            IsWalkinMinimized = false;
            PatientSearchQuery = "";
            WalkinCustid = 0;
            WalkinDate = DateTime.UtcNow.ToIndianTime().Date;
            WalkinDcode = 0;
            DutyDcode = 0;
            WalkinRegType = "OP";
            SelectedServiceId = 0;
            WalkinNotes = "";
            SelectedWalkinSlot = null;
            SelectedSlotDetailId = null;
            NotifyStateChanged();
        }
    }
}
