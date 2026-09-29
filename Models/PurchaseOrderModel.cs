using System;
using System.Collections.Generic;

namespace Booking.Models
{
    // ─── PURCHASE ORDER MASTER ─────────────────────────────────────────────
    public class purchase_order_master
    {
        public long ordercode { get; set; }

        public string? orderno { get; set; }
        public DateTime orderdate { get; set; } = DateTime.Today;
        public string? llrcode { get; set; }
        public long? vendorcode { get; set; }

        public bool overalltax { get; set; }
        public decimal taxpercentage { get; set; }

        public decimal grossamount { get; set; }
        public decimal discountamount { get; set; }
        public decimal taxamount { get; set; }
        public decimal roundoff { get; set; }
        public decimal grandtotal { get; set; }

        public string? status { get; set; } = "PENDING";  // PENDING / APPROVED / CLOSED / CANCELLED / CONVERTED

        public string? remarks { get; set; }

        public bool isactive { get; set; } = true;
        public bool deleted { get; set; } = false;

        public DateTime createddate { get; set; } = DateTime.Now;
        public DateTime? modifieddate { get; set; }
        public long? usercode { get; set; }

        public string? tenantcode { get; set; }
        public string? branchcode { get; set; }
        public string? companycode { get; set; }
    }

    // ─── PURCHASE ORDER DETAIL (item lines) ────────────────────────────────
    public class purchase_order_detail
    {
        public long orderdetailcode { get; set; }
        public long ordercode { get; set; }

        public long itemcode { get; set; }
        public int? itemgroupcode { get; set; }

        public decimal quantity { get; set; }
        public decimal packing { get; set; } = 1;
        public decimal freeqty { get; set; }
        public decimal mrp { get; set; }
        public decimal orderrate { get; set; }

        public decimal discountpercentage { get; set; }
        public decimal discountamount { get; set; }

        public decimal amount { get; set; }

        public string? tenantcode { get; set; }
    }

    // ─── PURCHASE ORDER TERMS & CONDITIONS ─────────────────────────────────
    public class purchase_order_terms
    {
        public long termcode { get; set; }
        public long ordercode { get; set; }
        public int slno { get; set; }
        public string? termcondition { get; set; }
        public string? value { get; set; }
        public string? tenantcode { get; set; }
    }

    // ─── COMPOSITE REQUEST (what the controller receives) ─────────────────
    public class purchase_order_request
    {
        public purchase_order_master master { get; set; } = new();
        public List<purchase_order_detail> details { get; set; } = new();
        public List<purchase_order_terms> terms { get; set; } = new();
    }
}