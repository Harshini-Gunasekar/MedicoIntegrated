using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Booking.Models
{
    public class purchase_entry_master
    {
        [Key]
        public long purchaseentrycode { get; set; }

        // GRN
        public string? grnno { get; set; }
        public DateTime grndate { get; set; } = DateTime.Now;
        public long? receivedby { get; set; }

        // Purchase
        public string? billno { get; set; }
        public DateTime? billdate { get; set; } = DateTime.Now;
        public string? invoiceno { get; set; }
        public DateTime? invoicedate { get; set; } = DateTime.Now;

        [Range(1, long.MaxValue, ErrorMessage = "Vendor selection is required")]
        public long vendorcode { get; set; }

        public decimal totalqty { get; set; }
        public decimal receivedqty { get; set; }

        public decimal grossamount { get; set; }
        public decimal discountamount { get; set; }
        public decimal taxamount { get; set; }
        public decimal othercharges { get; set; }
        public decimal netamount { get; set; }

        public string? paymentmode { get; set; }
        public string? paymentstatus { get; set; }

        public string? approvalstatus { get; set; }
        public bool posted { get; set; }

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

    public class purchase_entry_detail
    {
        [Key]
        public long purchaseentrydetailcode { get; set; }

        public long purchaseentrycode { get; set; }
        
        [Range(1, long.MaxValue, ErrorMessage = "Item selection is required")]
        public long itemcode { get; set; }

        public decimal orderedqty { get; set; }
        public decimal receivedqty { get; set; }
        public decimal rejectedqty { get; set; }
        public decimal quantity { get; set; }

        public decimal rate { get; set; }
        public decimal discountpercentage { get; set; }
        public decimal discountamount { get; set; }
        public decimal taxpercentage { get; set; }
        public decimal taxamount { get; set; }

        public decimal amount { get; set; }
        public decimal totalamount { get; set; }

        public string? batchno { get; set; }
        public DateTime? manufacturingdate { get; set; } = DateTime.Now;
        public DateTime? expirydate { get; set; } = DateTime.Now.AddYears(1);

        public long? warehousecode { get; set; }

        public string? tenantcode { get; set; }
    }

    public class purchase_entry_request
    {
        public purchase_entry_master master { get; set; } = new();
        public List<purchase_entry_detail> details { get; set; } = new();
    }
}
