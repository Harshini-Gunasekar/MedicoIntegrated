using System;
using SharedComponents.Rcl.Models;

namespace Booking.Models
{
    public class CustomerWithOpModel : CustomerModel
    {
        public string? op_id { get; set; }
        public string? op_no { get; set; }
        public string? booking_id { get; set; }
        public string? booking_no { get; set; }
        public string? slot_detail_id { get; set; }
        public string? visit_type { get; set; }
        public string? reg_type { get; set; }
        public string? visit_date { get; set; }
        public int? token_no { get; set; }
        public string? visit_status { get; set; }
        public string? notes { get; set; }
        public bool? is_direct_walkin { get; set; }
        public DateTime? op_created_at { get; set; }
        public DateTime? op_updated_at { get; set; }
    }
}
