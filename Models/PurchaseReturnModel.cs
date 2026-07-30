 using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class purchase_return_lookup_request
    {
        public long itemcode { get; set; }
        public string? batchno { get; set; }   
    }
    // Step 1 output: vendor + rate + available qty for that item/batch
    public class purchase_return_lookup_result
    {
        public long purchasedetailcode { get; set; }
        public long purchasecode { get; set; }
        public long itemcode { get; set; }
        public string itemname { get; set; }
        public string batchno { get; set; }

        public long vendorcode { get; set; }
        public string vendorname { get; set; }
        public string contactperson { get; set; }
        public string phonenumber { get; set; }
        public string gstnumber { get; set; }

        public decimal rate { get; set; }
        public decimal receivedqty { get; set; }
        public decimal returnedqty { get; set; }
        public decimal availableqty { get; set; }   // receivedqty - returnedqty
        public decimal packsize { get; set; }

        public long? warehousecode { get; set; }
    }

    // Step 2 input: user enters returnqty + packsize
    public class purchase_return_request
    {
        public long purchasedetailcode { get; set; }
        public long purchasecode { get; set; }
        public long itemcode { get; set; }
        public long vendorcode { get; set; }
        public string batchno { get; set; }

        public decimal returnqty { get; set; }
        public decimal packsize { get; set; }

        public long? warehousecode { get; set; }
        public string? remarks { get; set; }

        public long? usercode { get; set; }
        public string? tenantcode { get; set; }
    }

    // Persisted return record
    [Table("purchase_return_master")]
    public class purchase_return_master
    {
        [Key]
        public long purchasereturncode { get; set; }

        public long purchasedetailcode { get; set; }
        public long purchasecode { get; set; }
        public long itemcode { get; set; }
        public long vendorcode { get; set; }
        public string? batchno { get; set; }

        public decimal returnqty { get; set; }
        public decimal packsize { get; set; }
        public decimal totalqty { get; set; }   // returnqty * packsize
        public decimal rate { get; set; }
        public decimal amount { get; set; }     // totalqty * rate

        public long? warehousecode { get; set; }
        public string? remarks { get; set; }

        public bool isactive { get; set; }
        public bool deleted { get; set; }
        public DateTime createddate { get; set; }
        public long? usercode { get; set; }
        public string? tenantcode { get; set; }
    }
