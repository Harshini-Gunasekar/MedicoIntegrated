using System;

namespace LabCare.Models
{
    public class ForgetResponseModel
    {
        public string? token { get; set; }
        public string? otp { get; set; }
        public DateTime? tokenExpiry { get; set; }
        public DateTime? otpExpiry { get; set; }
        public string? message { get; set; }
        public bool status { get; set; }
        public ForgetData? data { get; set; }
    }

    public class ForgetData
    {
        public string? token { get; set; }
        public string? otp { get; set; }
        public DateTime? tokenExpiry { get; set; }
        public DateTime? otpExpiry { get; set; }
    }
}
