using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace TrackAdmin.Models
{
    public class StatusMonitor
    {
    }

    public class RiderStatus
    {
        public Guid ID { get; set; }
        public string Session { get; set; }
        public string latitude { get; set; }
        public string longitude { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public int RiderID { get; set; }
    }

    [Table(name: "Rider_Current_Location")]
    public class Rider_Current_Location
    {
        public int CurrentID { get; set; }
        public int RiderID { get; set; }
        public string Vehicle { get; set; }
        public Double Latitude { get; set; }
        public Double Longitude { get; set; }
        public string Status { get; set; }
        public string Gender { get; set; }
        public DateTime EnteredDate { get; set; }
    }

    [Table(name: "Rider_Master")]
    public class Rider_Master
    {
        public int RiderID { get; set; }
        public string ShortName { get; set; }
        [Required(ErrorMessage = "Rider Name is required")]
        public string Name { get; set; }
        public string Address { get; set; }
        [Required(ErrorMessage = "Mobile No is required")]
        public string Mobile { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid Email format")]
        public string Email { get; set; }
        public string PassportNo { get; set; }
        public string EmiratesID { get; set; }
        public string Status { get; set; }
        public string? StatusReason { get; set; }
        public string? CurrentStatus { get; set; }
        public bool Deleted { get; set; }
        public DateTime EnteredDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        [Required(ErrorMessage = "Date of Birth is required")]
        public DateTime DateofBirth { get; set; }
        public int Age { get; set; }
        public string AgeType { get; set; }
        public string Gender { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
        public string DrivingLicense { get; set; }
        public string RiderType { get; set; }

        public string VehicleType { get; set; }
    }

    public class Referral_Master
    {
        public int ReferralID { get; set; }
        public string ShortName { get; set; } = "";

        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; }
        public string BillingAddress { get; set; } = "";


        public string MobileNo { get; set; }
        public string ContactNo { get; set; } = "";


        public string Email { get; set; }
        public string ReferralType { get; set; } = "";
        public int DefaultRider { get; set; }
        public Double Latitude { get; set; } = 0;
        public Double Longitude { get; set; } = 0;
        public DateTime EnteredDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Status { get; set; } = "";
        public int Radius { get; set; } = 0;


        public string ContactPerson { get; set; }
        public string? Password { get; set; }
    }

    public class Appointments
    {
        public Guid Id { get; set; }
        public int AppSno { get; set; }
        public int? AppType { get; set; }
        public int? ReferralID { get; set; }
        public int? CustID { get; set; }
        public int? RiderID { get; set; }
        public DateTime? CalledTime { get; set; }
        public DateTime? AppointmentOn { get; set; }
        public bool? Deleted { get; set; }
        public string? Narration { get; set; }
        public DateTime? EnteredDate { get; set; }
        public string? Status { get; set; }
        public bool IsNext { get; set; }
        public int? AssignedBy { get; set; }
        public int? EscalationLevel { get; set; }
        public string? EscalationStatus { get; set; }
    }

    public class EscalationAlert
    {
        public int EscalationId { get; set; }
        public string ReferenceId { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public int AdminId { get; set; }
        public string RiderMobile { get; set; }
        public int AppSno { get; set; }
    }
    public class WhatsappAppointment
    {
        public string wapp_id { get; set; }
        public string wa_number { get; set; }
        public int? referral_id { get; set; }
        public string appointment_Text { get; set; }
        public string Status { get; set; }
        public int? rider_id { get; set; }
        public DateTime? appointment { get; set; }
        public DateTime? updated_Time { get; set; }
        public bool IsAvailable { get; set; }
     }
 
     public class SampleCollection_Master
    {
        public Guid SPGuid { get; set; }
        public int SPSno { get; set; }
        public DateTime SPDate { get; set; }

        public int ReferralID { get; set; }
        public int RiderID { get; set; }
        public DateTime CalledTime { get; set; }
        public DateTime CollectedTime { get; set; }
        public DateTime? ReceivedTime { get; set; }
        public bool TRF { get; set; }
        public int DefaultRiderID { get; set; }
        public bool IsCollected { get; set; }
        public bool IsReceived { get; set; }
        public DateTime? InTime { get; set; }
        public DateTime? OutTime { get; set; }
        public bool Deleted { get; set; }
        public string Narration { get; set; }
        public bool IsUrgent { get; set; }
        public bool IsRoutine { get; set; }
        public bool IsAmbient { get; set; }
        public bool IsRefrigerated { get; set; }
        public bool IsFrozen { get; set; }
        public string InitialTemp { get; set; }
        public string CollectionTemp { get; set; }
        public Double Amount { get; set; }

        public Guid AppID { get; set; }
        public Guid TripID { get; set; }
        public int? CollectedQty { get; set; }
        public int? ReceivedQty { get; set; }
        public int? RejectedQty { get; set; }
        public string QtyReasons { get; set; }
    }

    public class SampleCollection_Details
    {
        public Guid SPDGuid { get; set; }
        public Guid SPGuid { get; set; }

        public int SCode { get; set; }
        public string Color { get; set; }
        public string Container { get; set; }

        public string Sample { get; set; }
        public int Quantity { get; set; }
        public string Temp { get; set; }
        public string Narration { get; set; }
        public int AcceptQty { get; set; }
        public int RejectedQty { get; set; }
        public string Reasons { get; set; }
    }

    public class SampleCollectionRequest
    {
        public SampleCollection_Master scm { get; set; }
        public IList<SampleCollection_Details> scd { get; set; }
    }

    public class TripMaster
    {
        public Guid TripID { get; set; }
        public DateTime TripDate { get; set; }
        public int TripNo { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? StopTime { get; set; }

        public Double? StartLatitude { get; set; }
        public Double? StartLongitude { get; set; }
        public Double? StopLatitude { get; set; }
        public Double? StopLongitude { get; set; }
        public bool Deleted { get; set; }
        public int RiderID { get; set; }

        public Double? Distance { get; set; }

        public string StartTemp { get; set; }
        public string EndTemp { get; set; }
        public bool IsReturn { get; set; }
    }
    public class LoadLocation
    {
        public Rider_Current_Location rcl { get; set; }
        public RiderStatus rs { get; set; }
    }

    public class TripDetails
    {
        public Guid TripDetailsID { get; set; }
        public Guid TripID { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public Double DistanceKM { get; set; }
        public int DurationMinutes { get; set; }
        public bool Deleted { get; set; }
        public int RiderID { get; set; }
        public int ReferralID { get; set; }

        public Double StartLatitude { get; set; }
        public Double StartLongitude { get; set; }

        public Double EndLatitude { get; set; }
        public Double EndLongitude { get; set; }
    }



    public class TripView
    {
        public Guid TripID { get; set; }
        public DateTime TripDate { get; set; }
        public int TripNo { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public Double StartLatitude { get; set; }
        public Double StartLongitude { get; set; }
        public Double EndLatitude { get; set; }
        public Double EndLongitude { get; set; }
        public int RiderID { get; set; }

        public int ReferralID { get; set; }
        public bool IsReturn { get; set; }
        public Guid TripDetailsID { get; set; }
        public Double DistanceKM { get; set; }
        public int DurationMinutes { get; set; }
    }

    public class SampleTransferResponse
    {
        public int Id { get; set; }
        public int STId { get; set; }
        public Guid? SPGuid { get; set; }
        public int RiderId { get; set; }
        public string SendingType { get; set; }
        public string BusNumber { get; set; }
        public string DriverName { get; set; }
        public string DriverNumber { get; set; }
        public string Narration { get; set; }
        public string FolderPath { get; set; }
        public int? ToRiderId { get; set; }
        public string SenderName { get; set; }
        public string ReceiverName { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? IsAccepted { get; set; }
        public string RejectionReason { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public string Status { get; set; }
        public List<string> Photos { get; set; } = new List<string>();
        public List<string> ImageUrls { get; set; } = new List<string>();
    }

    public class ChatMessage
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int? RecipientId { get; set; }
        public int? GroupId { get; set; }
        public string MessageType { get; set; }
        public string Content { get; set; }
        public string FilePath { get; set; }
        public bool IsRead { get; set; }
        public bool IsDelivered { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
