using System;
using System.Collections.Generic;

namespace Booking.Models
{
    public class stock_transfer_master
    {
        public long transfercode { get; set; }
        public string? transferno { get; set; }
        public DateTime transferdate { get; set; } = DateTime.Today;
        public string? transfertype { get; set; } = "Standard";
        public long? indentcode { get; set; }
        public long? fromwarehousecode { get; set; }
        public long? towarehousecode { get; set; }
        public string? branchcode { get; set; }
        public string? reason { get; set; } = "Stock Requirement";
        public string? remarks { get; set; }
        public string? status { get; set; } = "DRAFT"; // DRAFT, PENDING, COMPLETED, REJECTED, CANCELLED
        public long? createdby { get; set; }
        public DateTime? createddate { get; set; } = DateTime.Now;
        public string? tenantcode { get; set; }

        public List<stock_transfer_detail>? details { get; set; }
    }

    public class stock_transfer_detail
    {
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
        public string? tenantcode { get; set; }
    }

    public class stock_transfer_request
    {
        public stock_transfer_master master { get; set; } = new stock_transfer_master();
        public List<stock_transfer_detail> details { get; set; } = new List<stock_transfer_detail>();
    }

    public class stock_transfer_action_request
    {
        public long transfercode { get; set; }
        public long usercode { get; set; }
        public string? remarks { get; set; }
    }
}
