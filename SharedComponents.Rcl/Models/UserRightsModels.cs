using System;
using System.Collections.Generic;

namespace SharedComponents.Rcl.Models
{
    public class ProductList
    {
        public int Id { get; set; }
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string BaseUrl { get; set; }
        public string IconCss { get; set; }
        public string TenantCode { get; set; }
        public bool IsActive { get; set; }
        public DateTime? SubscriptionStart { get; set; }
        public DateTime? SubscriptionEnd { get; set; }
        public int MaxUsers { get; set; }
        public string Features { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ProductFeature
    {
        public int FeatureId { get; set; }
        public string ProductId { get; set; }
        public string FeatureKey { get; set; }
        public string FeatureName { get; set; }
        public string FeatureDescription { get; set; }
        public bool HasView { get; set; }
        public bool HasCreate { get; set; }
        public bool HasEdit { get; set; }
        public bool HasDelete { get; set; }
        public bool HasExport { get; set; }
        public bool HasChart { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class RoleTemplate
    {
        public string RoleName { get; set; }
        public string RightsJson { get; set; }
    }

    public class ProductPermission
    {
        public int UserID { get; set; }
        public string TenantID { get; set; }
        public string ProductId { get; set; }
        public string RoleName { get; set; }
        public string RightsJson { get; set; }
    }

    public class User_Master
    {
        public int UserID { get; set; }
        public int OrderNo { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public string Mobile { get; set; }
        public string manager_mobile { get; set; }
        public string Email { get; set; }
        public string Narration { get; set; }
        public bool IsPowerUser { get; set; }
        public bool Deleted { get; set; }
        public DateTime EnteredDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Password { get; set; }
    }

    public class TenantProfile
    {
        public string TenantId { get; set; }
        public string TenantCode { get; set; }
        public string TenantName { get; set; }
        public string LegalName { get; set; }
        public string BusinessType { get; set; }
        public string RegisterNum { get; set; }
        public string ContactEmail { get; set; }
        public string ContactNumber { get; set; }
        public string ContactPerson { get; set; }
        public string AlternateMobile { get; set; }
        public string Website { get; set; }
        public string GstNumber { get; set; }
        public string PanNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Pincode { get; set; }
        public string TimeZone { get; set; } = "Asia/Kolkata";
        public string Currency { get; set; } = "INR";
        public string LogoUrl { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class BranchMaster
    {
        public int BhCode { get; set; }
        public int OrderNo { get; set; }
        public string ShortName { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Pincode { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Description { get; set; }
        public int? AreaCode { get; set; }
        public bool Deleted { get; set; }
        public int UserCode { get; set; }
        public int ComputerCode { get; set; }
        public DateTime EnteredDate { get; set; }
        public DateTime IbsDate { get; set; }
        public bool? IsMainBranch { get; set; }
        public bool? IsBranch { get; set; }
        public bool? IsCollectionCentre { get; set; }
        public string PharmacyName { get; set; }
        public string LabName { get; set; }
        public string TenantCode { get; set; }
    }
}

