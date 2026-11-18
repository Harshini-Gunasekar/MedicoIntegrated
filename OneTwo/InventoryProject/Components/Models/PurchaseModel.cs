using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Inventory_management_frontend.Models
{
    public class PurchaseMasterModel
    {
   
        public string? billno { get; set; }
        
        public long? purchasecode { get; set; }

        [Required(ErrorMessage = "Bill Date is required")]
         public DateTime? billdate { get; set; }
             
        public string? invoiceno { get; set; }
        
        public DateTime? invoicedate { get; set; }
        
       
        public int? vendorcode { get; set; }
        
        public decimal? grossamount { get; set; }
        
        public decimal? discountamount { get; set; }
        
        public decimal? taxamount { get; set; }
        
        public decimal? netamount { get; set; }
               
        public decimal? roundoff { get; set; }
                
        public string? paymentmode { get; set; }
                
        public string? paymentstatus { get; set; }
    
        public string? currencycode { get; set; }
        
     
        public string? remarks { get; set; }
        
       
        public bool isactive { get; set; } = true;
        
       
        public bool deleted { get; set; } = false;
        
     
        public int? usercode { get; set; }
        
        
        public string? tenantcode { get; set; } = "TEN1011";
        
        
        public string? branchcode { get; set; }
        
   
        public string? companycode { get; set; }

       
        public DateTime? createddate { get; set; }

   
        public DateTime? modifieddate { get; set; }

        public long grncode { get; set; }

        public long? warehousecode { get; set; }

        public List<PurchaseDetailModel>? details { get; set; }
        public decimal transportationcharges { get; set; }
    }

    public class PurchaseDetailModel
    {
       
        public long? purchasedetailcode { get; set; }

        
        public int itemcode { get; set; }

       
        public decimal quantity { get; set; }

       
        public decimal freequantity { get; set; }

    
        public int uomcode { get; set; }

      
        public decimal rate { get; set; }

   
        public decimal discountpercentage { get; set; }

        public decimal discountamount { get; set; }


        public decimal taxpercentage { get; set; }

       
        public decimal taxamount { get; set; }

        
        public decimal amount { get; set; }

       
        public long? purchasecode { get; set; }

        
        public decimal totalamount { get; set; }

        
        public string? batchno { get; set; }

        public DateTime? manufacturingdate { get; set; }

        public DateTime? expirydate { get; set; }

        public decimal orderedqty { get; set; }

        public decimal receivedqty { get; set; }

        public decimal rejectedqty { get; set; }

        public long? warehousecode { get; set; }

        // Packaging
        public string? packaging { get; set; }

        // Manufacturer
        public long? manufacturercode { get; set; }
        public string? tenantcode { get; set; } = "TEN1011";
    }

    public class PurchaseRequest
    {
       
        public PurchaseMasterModel master { get; set; } = new();

        public List<PurchaseDetailModel> details { get; set; } = new();
    }
}
