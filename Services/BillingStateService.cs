using System;
using System.Collections.Generic;
using Booking.Models;

namespace Booking.Services
{
    public class BillingStateService
    {
        public event Action? OnChange;
        public void NotifyStateChanged() => OnChange?.Invoke();

        public HmsBillModel? BillModel { get; set; }
        public bool HasState { get; set; } = false;

        public string PatientPrefix { get; set; } = "Mr.";
        public string CareOfPrefix { get; set; } = "S/O";
        public string CareOfName { get; set; } = string.Empty;
        public string PatientEmail { get; set; } = string.Empty;
        public DateOnly? DobSelected { get; set; } = null;
        public string PatientSearchQuery { get; set; } = string.Empty;
        public bool SelectedPatientBadgeVisible { get; set; } = false;
        public string? SelectedOpNo { get; set; } = "-";
        public string? SelectedIpNo { get; set; } = "-";
        public List<UnbilledChargeSummary> UnbilledCharges { get; set; } = new();
        public bool ShowPaymode2 { get; set; } = false;
        public bool ShowPaymode3 { get; set; } = false;
        public bool ShowAddConsultationForm { get; set; } = false;
        public double ConsultationRate { get; set; } = 250;
        public double ConsultationQty { get; set; } = 1;
        public bool IsHuman { get; set; } = true;
        public bool IsVipPatient { get; set; } = false;
        public bool ShowMoreInfo { get; set; } = false;

        public string GetBillingPatientName()
        {
            if (BillModel != null && !string.IsNullOrWhiteSpace(BillModel.patient_name))
            {
                return BillModel.patient_name.Trim();
            }
            if (!string.IsNullOrWhiteSpace(PatientSearchQuery))
            {
                var query = PatientSearchQuery.Trim();
                int idx = query.IndexOf('(');
                if (idx > 0) query = query.Substring(0, idx).Trim();
                return query;
            }
            return string.Empty;
        }

        public void SaveState(
            HmsBillModel billModel,
            string patientPrefix,
            string careOfPrefix,
            string careOfName,
            string patientEmail,
            DateOnly? dobSelected,
            string patientSearchQuery,
            bool selectedPatientBadgeVisible,
            string? selectedOpNo,
            string? selectedIpNo,
            List<UnbilledChargeSummary> unbilledCharges,
            bool showPaymode2,
            bool showPaymode3,
            bool showAddConsultationForm,
            double consultationRate,
            double consultationQty,
            bool isHuman,
            bool isVipPatient,
            bool showMoreInfo)
        {
            BillModel = billModel;
            PatientPrefix = patientPrefix;
            CareOfPrefix = careOfPrefix;
            CareOfName = careOfName;
            PatientEmail = patientEmail;
            DobSelected = dobSelected;
            PatientSearchQuery = patientSearchQuery;
            SelectedPatientBadgeVisible = selectedPatientBadgeVisible;
            SelectedOpNo = selectedOpNo;
            SelectedIpNo = selectedIpNo;
            UnbilledCharges = unbilledCharges != null ? new List<UnbilledChargeSummary>(unbilledCharges) : new();
            ShowPaymode2 = showPaymode2;
            ShowPaymode3 = showPaymode3;
            ShowAddConsultationForm = showAddConsultationForm;
            ConsultationRate = consultationRate;
            ConsultationQty = consultationQty;
            IsHuman = isHuman;
            IsVipPatient = isVipPatient;
            ShowMoreInfo = showMoreInfo;
            HasState = true;
            NotifyStateChanged();
        }

        public void Clear()
        {
            BillModel = null;
            PatientPrefix = "Mr.";
            CareOfPrefix = "S/O";
            CareOfName = string.Empty;
            PatientEmail = string.Empty;
            DobSelected = null;
            PatientSearchQuery = string.Empty;
            SelectedPatientBadgeVisible = false;
            SelectedOpNo = "-";
            SelectedIpNo = "-";
            UnbilledCharges.Clear();
            ShowPaymode2 = false;
            ShowPaymode3 = false;
            ShowAddConsultationForm = false;
            ConsultationRate = 250;
            ConsultationQty = 1;
            IsHuman = true;
            IsVipPatient = false;
            ShowMoreInfo = false;
            HasState = false;
            NotifyStateChanged();
        }
    }
}
