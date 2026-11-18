using System;
using System.ComponentModel.DataAnnotations;

namespace TenantMasterRegistration.Models
{
    public class UserMasterModel
    {
        public int? UserID { get; set; }
        public int? OrderNo { get; set; }
        
        [Required(ErrorMessage = "Short Name is required")]
        public string? ShortName { get; set; }
        
        [Required(ErrorMessage = "Name is required")]
        public string? Name { get; set; }
        
        [Required(ErrorMessage = "Mobile number is required")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Mobile must be exactly 10 digits")]
        public string? Mobile { get; set; }
        
        public string? manager_mobile { get; set; }
        
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        public string? Email { get; set; }
        
        public string? Narration { get; set; }
        public bool? IsPowerUser { get; set; }
        public bool? Deleted { get; set; }
        public DateTime? EnteredDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
        public string? Password { get; set; }

        // Global Identity Fields
        public string? spouse_name { get; set; }
        public string? father_name { get; set; }
        public string? mother_name { get; set; }
        
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Alternate Mobile must be exactly 10 digits")]
        public string? mobile_alternate { get; set; }
        
        public string? permanent_address { get; set; }
        public string? current_address { get; set; }
        public string? id_document_path { get; set; }
        public string? global_user_id { get; set; }
        
        public DateTime? dob { get; set; }
        public string? PassportNo { get; set; }
        public string? EmiratesID { get; set; }
        public int? Age { get; set; }
        public string? AgeType { get; set; } = "Years";
        
        [Required(ErrorMessage = "Gender is required")]
        public string? Gender { get; set; }
        
        public string? Status { get; set; } = "Active";
    }
}
