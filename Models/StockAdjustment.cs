using System;

namespace Booking.Models
{
    public class stock_adjustment_stock_ref
    {
        public long stockcode { get; set; }
    }

    public class stock_adjustment_log
    {
        public long adjustmentlogcode { get; set; }

        public long stockcode { get; set; }

        public long itemcode { get; set; }

        public long? warehousecode { get; set; }

        public string? branchcode { get; set; }

        public string? locationcode { get; set; }

        public string? batchno { get; set; }

        // Stock before adjustment
        public decimal beforeqty { get; set; }

        // Adjustment quantity (+/-)
        public decimal adjustedqty { get; set; }

        // Stock after adjustment
        public decimal afterqty { get; set; }

        public decimal unitcost { get; set; }

        public decimal stockvalue { get; set; }

        // Increase / Decrease
        public string adjustmenttype { get; set; } = string.Empty;

        // Physical Count, Damage, Expiry, Theft, etc.
        public string adjustmentreason { get; set; } = string.Empty;

        public string? remarks { get; set; }

        public DateTime adjustmentdate { get; set; }

        // User who performed the adjustment
        public long? adjustedby { get; set; }

        public bool isactive { get; set; } = true;

        public bool deleted { get; set; } = false;

        public DateTime createddate { get; set; }

        public DateTime? modifieddate { get; set; }

        public string? tenantcode { get; set; }

        public string? companycode { get; set; }
    }

    public class stock_adjustment_request
    {
        public stock_master stock { get; set; } = new();

        public stock_adjustment_log adjustmentlog { get; set; } = new();
    }

    public class stock_adjustment_upsert_payload
    {
        public stock_adjustment_stock_ref stock { get; set; } = new();

        public stock_adjustment_log adjustmentlog { get; set; } = new();
    }
}