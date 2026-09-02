using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Booking.Models
{
    public class PurchaseMasterModel
    {
   
    public long purchasecode { get; set; }

    public string? billno { get; set; }

    public DateTime billdate { get; set; }

    public string? invoiceno { get; set; }

    public DateTime? invoicedate { get; set; }

    public long? vendorcode { get; set; }

    public decimal grossamount { get; set; }

    public decimal discountamount { get; set; }

    public decimal taxamount { get; set; }

    public decimal netamount { get; set; }
    public decimal overalldiscount { get; set; }     // overall discount % applied on grossamount
   
    public decimal totalamount { get; set; }


    // New Fields
    public decimal transportationcharges { get; set; }

    public decimal roundoff { get; set; }

    public string? paymentmode { get; set; }

    public string? paymentstatus { get; set; }

    public string? currencycode { get; set; }

    public bool isactive { get; set; }

    public bool deleted { get; set; }

    public string? remarks { get; set; }

    public DateTime createddate { get; set; }

    public DateTime? modifieddate { get; set; }

    public long? usercode { get; set; }

    public string? tenantcode { get; set; }

    public string? branchcode { get; set; }

    public string? companycode { get; set; }

    public long grncode { get; set; }
    }

    public class PurchaseDetailModel
    {
        public long purchasedetailcode { get; set; }

        public long purchasecode { get; set; }

        public long itemcode { get; set; }

        public decimal quantity { get; set; }

        public decimal freequantity { get; set; }

        public long? uomcode { get; set; }

        public decimal rate { get; set; }

        public decimal discountpercentage { get; set; }

        public decimal discountamount { get; set; }

        public decimal taxpercentage { get; set; }

        public decimal taxamount { get; set; }

        public decimal amount { get; set; }

        public decimal totalamount { get; set; }

        public string? batchno { get; set; }

        public DateTime? manufacturingdate { get; set; }

        public DateTime? expirydate { get; set; }

        // New Fields

        public decimal orderedqty { get; set; }

        public decimal receivedqty { get; set; }

        public decimal rejectedqty { get; set; }
        public decimal returnedqty { get; set; }
        public decimal issuedqty { get; set; }

        // Warehouse / Store
        public long warehousecode { get; set; }

        // Packaging
        public string? packaging { get; set; }

        // Manufacturer
        public long manufacturercode { get; set; }

        public string? tenantcode { get; set; }
        public int packsize { get; set; }
        public decimal packg { get; set; }
        public decimal mrp { get; set; }
    }

    public class PurchaseRequest
    {
        public PurchaseMasterModel master { get; set; }
        public List<PurchaseDetailModel> details { get; set; }

    }
}
