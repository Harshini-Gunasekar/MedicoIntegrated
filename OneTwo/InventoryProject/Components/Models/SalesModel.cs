    public class sales_master
    {
        public long salescode { get; set; }

        public string? billno { get; set; }

        public DateTime billdate { get; set; }

        public string? invoiceno { get; set; }

        public DateTime? invoicedate { get; set; }

        public long? customercode { get; set; }

        public decimal grossamount { get; set; }

        public decimal discountamount { get; set; }

        public decimal taxamount { get; set; }

        public decimal netamount { get; set; }

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

        public long? ordercode { get; set; }
    }
    public class sales_detail
    {
        public long salesdetailcode { get; set; }

        public long salescode { get; set; }

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

        // Sales Specific

        public decimal orderedqty { get; set; }

        public decimal deliveredqty { get; set; }

        public decimal returnedqty { get; set; }

        public long? warehousecode { get; set; }

        public string? tenantcode { get; set; }
    }
    public class sales_request
    {
        public sales_master master { get; set; }
        public List<sales_detail> details { get; set; }
    }
