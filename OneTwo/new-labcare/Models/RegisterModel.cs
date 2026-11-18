using System.ComponentModel.DataAnnotations;

namespace LabCare.Models
{
    public class RegisterModel
    {
        // Step 1: Organization Information
        public string tenant_code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Laboratory Name is required")]
        public string tenant_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Legal Name is required")]
        public string legal_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Lab Type is required")]
        public string business_type { get; set; } = string.Empty;

        public string? register_num { get; set; }
        public string? gst_number { get; set; }
        public string? pan_number { get; set; }
        public string? website { get; set; }

        // Step 2: Contact Information
        [Required(ErrorMessage = "Contact Person Name is required")]
        public string contact_person { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email Address is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address (must contain @)")]
        public string contact_email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact Number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Contact Number must be exactly 10 digits (numeric only)")]
        public string contact_number { get; set; } = string.Empty;

        public string? alternate_mobile { get; set; }

        public string? address_line1 { get; set; }

        public string? address_line2 { get; set; }

        public string? city { get; set; }

        public string? state { get; set; }

        public string? pincode { get; set; }

        public string? time_zone { get; set; } = "Asia/Kolkata";
        public string? currency { get; set; } = "INR";
        public string? country { get; set; } = "India";

        // Step 3: Password
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$",
            ErrorMessage = "Password must contain uppercase, lowercase, a number, and a special character.")]
        public string password_hash { get; set; } = string.Empty;

    }
}
