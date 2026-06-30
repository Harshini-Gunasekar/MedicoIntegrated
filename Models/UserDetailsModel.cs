using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LabCare.Models
{
    public class UserDetailsModel
    {
        public string? UserName { get; set; }
        public string? Role { get; set; }
        public string? TenantCode { get; set; }
        public string? TenantName { get; set; }
        public string? AuthToken { get; set; }
        public int? UserCode { get; set; }
        public int? BranchCode { get; set; }
        public int? CounterCode { get; set; }

        public object? Claim { get; set; }
        public object? UserRights { get; set; }
        public IList<BranchModel>? Branches { get; set; }
        public object? UserDetails { get; set; }
    }
}
