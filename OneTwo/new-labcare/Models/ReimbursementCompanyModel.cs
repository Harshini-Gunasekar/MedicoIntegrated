using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabCare.Models
{
    [Table("reimbursement_company_master")]
    public class ReimbursementCompanyModel
    {
        [Key]
        public long ricode { get; set; }
        public int orderno { get; set; }
        public int ftcode { get; set; }
        public string? shortname { get; set; }
        public string? name { get; set; }
        public string? address { get; set; }
        public string? city { get; set; }
        public string? pincode { get; set; }
        public string? state { get; set; }
        public string? country { get; set; }
        public string? phone { get; set; }
        public string? contactname { get; set; }
        public string? mobile { get; set; }
        public string? fax { get; set; }
        public string? email { get; set; }
        public string? website { get; set; }
        public string? description { get; set; }
        public int? areacode { get; set; }
        public bool deleted { get; set; }
        public int usercode { get; set; }
        public int computercode { get; set; }
        public DateTime entereddate { get; set; }
        public DateTime ibsdate { get; set; }
        public string? lcode { get; set; }
        public int? bhcode { get; set; }
        public string? tenant_code { get; set; }
    }
}
