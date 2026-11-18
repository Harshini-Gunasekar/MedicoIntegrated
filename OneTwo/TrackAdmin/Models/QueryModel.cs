namespace TrackAdmin.Models
{
    public class QueryModel
    {
    }

    public class GetRiderStatusQuery
    {
        public int RiderID { get; set; }
        public string fd { get; set; }
        public string td { get; set; }
    }
    public class LoginDetailsModel
    {
        public string TenantID { get; set; }
        public string LoginId { get; set; }   // username / mobile / email
        public string Password { get; set; }
        public bool IsMobile { get; set; }
    }
    public class LoginResponse
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string TenantID { get; set; }
    }
    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string? Mobile { get; set; }
        public string? Email { get; set; }
        public string? token { get; set; }
        public int? UserCode { get; set; }
    }

    public class RiderLocation
    {
        public string RiderId { get; set; }
        public string RiderName { get; set; }
        public string FullName { get; set; }
        public string vehicleType { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Status { get; set; }
        public string? StatusReason { get; set; }
        public string? CurrentStatus { get; set; }

        public string Gender { get; set; }
        public string ProfilePicture { get; set; }
        public int UnreadCount { get; set; }
        public DateTime? LastMessageTime { get; set; }

    }

    public class ConfirmDialogOptions
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string OkButtonText { get; set; }
        public string CancelButtonText { get; set; }
        public bool ShowCloseIcon { get; set; }
    }

    public class Multipleids
    {
        public string uids { get; set; }
    }

    public class Location
    {
        public Double Latitude { get; set; }
        public Double Longituder { get; set; }
    }

    public class ReportModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int RiderID { get; set; }
        public int ReferralID { get; set; }

        public string ReportName { get; set; }

        public string Type { get; set; }
    }

    public class RiderVisit
    {
        public int RiderID { get; set; }
        public int ReferralID { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutTime { get; set; }
        public double TotalMinutes { get; set; }
    }
}
