using Dapper.Contrib.Extensions;
using System;

namespace LabCare.Models
{
    [Dapper.Contrib.Extensions.Table("tenants")]
    public class Tenant
    {
        [ExplicitKey]
        public Guid tenant_id { get; set; }

        public string tenant_code { get; set; } = string.Empty;
        public string tenant_name { get; set; } = string.Empty;

        public string contact_email { get; set; } = string.Empty;
        public string contact_number { get; set; } = string.Empty;

        public string legal_name { get; set; } = string.Empty;
        public string contact_person { get; set; } = string.Empty;
        public string? alternate_mobile { get; set; }
        public string? website { get; set; }

        public string? gst_number { get; set; }
        public string? pan_number { get; set; }

        public string address_line1 { get; set; } = string.Empty;
        public string? address_line2 { get; set; }
        public string? city { get; set; }
        public string? state { get; set; }
        public string? country { get; set; }
        public string? pincode { get; set; }

        public string? time_zone { get; set; } = "Asia/Kolkata";
        public string? currency { get; set; } = "INR";

        public string business_type { get; set; } = string.Empty;
        public string register_num { get; set; } = string.Empty;

        public bool? isdeleted { get; set; } = false;
        public bool is_active { get; set; } = true;

        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public string password_hash { get; set; } = string.Empty;
    }

    public class ForgotPasswordModel
    {
        public string Email { get; set; }
    }

    public class ResetPasswordModel
    {
        public string Token { get; set; }
        public string Otp { get; set; }
        public string Email { get; set; }
        public string NewPassword { get; set; }
    }

    public class ForgotPasswordResponse
    {
        public string Token { get; set; }
        public string Otp { get; set; }
        public DateTime TokenExpiry { get; set; }
        public DateTime OtpExpiry { get; set; }
    }
}
