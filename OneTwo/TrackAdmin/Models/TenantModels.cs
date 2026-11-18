using System;

namespace TrackAdmin.Models
{
    public class Tenant
    {
        public int Id { get; set; }
        public string TenantId { get; set; }
        public string TenantName { get; set; }
        public string ConnectionString { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string ClientCode { get; set; }
        public string LogoUrl { get; set; }
        public string HostUrl { get; set; }
    }

    // TenantProfile has moved to SharedComponents.Rcl.Models — use that instead.
    // BranchMaster has moved to SharedComponents.Rcl.Models — use that instead.

    public class UserMaster
    {
        public int UserCode { get; set; }
        public string GlobalUserId { get; set; } // Universal Key for SSO
        public int IbsdCode { get; set; }
        public int OrderNo { get; set; }
        public string Module { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Password { get; set; }
        public string Description { get; set; }
        public bool PowerUser { get; set; }
        public string UserImage { get; set; }
        public string SignatureImage { get; set; }
        public bool Deleted { get; set; }
        public int EuserCode { get; set; }
        public int ComputerCode { get; set; }
        public DateTime EnteredDate { get; set; }
        public DateTime IbsDate { get; set; }
        public int? BhCode { get; set; }
        public decimal? CntCode { get; set; }
        public decimal? HdCode { get; set; }
        public decimal? OpcntCode { get; set; }
        public decimal? IpcntCode { get; set; }
        public decimal? PharmacntCode { get; set; }
        public bool? IsConsultant { get; set; }
        public int? ConsultantdCode { get; set; }
        public string TenantCode { get; set; }
    }
}
