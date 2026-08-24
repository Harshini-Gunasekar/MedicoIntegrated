using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Booking.Models
{
    public class store_transfer_header
    {
        [Key]
        public long transfercode { get; set; }

        public string? transferno { get; set; }

        public DateTime transferdate { get; set; } = DateTime.Today;

        public string transfertype { get; set; } = "Standard";

        public long? indentcode { get; set; }

        public long? fromwarehousecode { get; set; }

        public long? towarehousecode { get; set; }

        public string? branchcode { get; set; }

        public string? reason { get; set; }

        public string? remarks { get; set; }

        public string status { get; set; } = "PENDING"; // DRAFT, PENDING, IN-TRANSIT, COMPLETED, CANCELLED

        public string? createdby { get; set; }

        public DateTime createddate { get; set; } = DateTime.Now;

        public bool isactive { get; set; } = true;

        public bool deleted { get; set; } = false;

        public string? tenantcode { get; set; }
    }

    public class store_transfer_detail
    {
        [Key]
        public long transferdetailcode { get; set; }

        public long transfercode { get; set; }

        public long itemcode { get; set; }

        public string? batchno { get; set; }

        public DateTime? manufacturingdate { get; set; }

        public DateTime? expirydate { get; set; }

        public long? uomcode { get; set; }

        public decimal availableqty { get; set; }

        public decimal requestedqty { get; set; }

        public decimal approvedqty { get; set; }

        public decimal transferqty { get; set; }

        public decimal unitcost { get; set; }

        public decimal stockvalue { get; set; }

        public string? remarks { get; set; }
    }

    public class store_transfer_request
    {
        public store_transfer_header master { get; set; } = new();
        public List<store_transfer_detail> details { get; set; } = new();
    }

    public class store_transfer_view_model
    {
        public store_transfer_header header { get; set; } = new();
        public List<store_transfer_detail> details { get; set; } = new();
        public string fromwarehousename { get; set; } = string.Empty;
        public string towarehousename { get; set; } = string.Empty;
        public string indentno { get; set; } = string.Empty;
        public int totalitems => details?.Count ?? 0;
        public decimal totalqty => details?.Sum(d => d.transferqty) ?? 0;
        public decimal totalvalue => details?.Sum(d => d.stockvalue) ?? 0;
    }
}
