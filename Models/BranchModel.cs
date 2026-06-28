using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Booking.Models
{
    [Table("branch_master")]
    public class BranchModel
    {
        [Key]
        public int bh_code { get; set; }
        public int order_no { get; set; }
        public string? short_name { get; set; }
        public string name { get; set; } = "";
        public string? address { get; set; }
        public string? city { get; set; }
        public string? pincode { get; set; }
        public string? state { get; set; }
        public string? country { get; set; }
        public string? phone { get; set; }
        public string? mobile { get; set; }
        public string? fax { get; set; }
        public string? email { get; set; }
        public string? website { get; set; }
        public string? description { get; set; }
        public int? area_code { get; set; }
        public bool deleted { get; set; } = false;
        public int user_code { get; set; }
        public int computer_code { get; set; }
        public DateTime entered_date { get; set; }
        public DateTime ibs_date { get; set; }
        public bool? is_main_branch { get; set; }
        public bool? is_branch { get; set; }
        public bool? is_collection_centre { get; set; }
        public string? pharmacy_name { get; set; }
        public string? lab_name { get; set; }
        public string? tenant_code { get; set; }
        public int? ftcode { get; set; }

        // Backward compatibility properties for other pages
        [JsonIgnore]
        public int? bhcode { get => bh_code; set => bh_code = value ?? 0; }
        [JsonIgnore]
        public int orderno { get => order_no; set => order_no = value; }
        [JsonIgnore]
        public string? shortname { get => short_name; set => short_name = value; }
        [JsonIgnore]
        public int? areacode { get => area_code; set => area_code = value; }
        [JsonIgnore]
        public DateTime? entereddate { get => entered_date; set => entered_date = value ?? DateTime.Now; }
        [JsonIgnore]
        public DateTime? ibsdate { get => ibs_date; set => ibs_date = value ?? DateTime.Now; }
        [JsonIgnore]
        public bool? ismainbranch { get => is_main_branch; set => is_main_branch = value; }
        [JsonIgnore]
        public bool? isbranch { get => is_branch; set => is_branch = value; }
        [JsonIgnore]
        public bool? iscollectioncentre { get => is_collection_centre; set => is_collection_centre = value; }
        [JsonIgnore]
        public int? usercode { get => user_code; set => user_code = value ?? 0; }
    }
}
