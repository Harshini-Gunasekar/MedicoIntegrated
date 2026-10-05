 public class sales_master
 {
     public long salescode { get; set; }

     public string? billno { get; set; }

     public DateTime billdate { get; set; }

     public string? invoiceno { get; set; }

     public DateTime? invoicedate { get; set; }

     public long? customercode { get; set; }

     // NEW FIELDS
     public string? salestype { get; set; }      // IP/OP or Counter Sales

     public string? warehousefield { get; set; }  // Warehouse

     public string? patientid { get; set; }      // Patient ID

     public string? mobileno { get; set; } 

     public string? patientname { get; set; }    // Patient Name

     public string? customername { get; set; }   // Customer / Patient Name Alias

     public string? address { get; set; }        // Patient Address

     public string? consultant { get; set; }     // Consultant Name

    public int? prescribeddays { get; set; }   // total days prescribed by doctor
    
    public int? converteddays { get; set; }

    public int? days { get; set; }

    public string? duration { get; set; }

     public decimal grossamount { get; set; }

     public decimal discountpercentage { get; set; }

     public decimal discountamount { get; set; }

     public decimal taxamount { get; set; }

     public decimal netamount { get; set; }

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

     public long? ordercode { get; set; }
     public decimal? cntcode { get; set; } = 0;
     public int? bh_code { get; set; }
     public string? cnttid { get; set; }   // counter shift session id
 }
 public class sales_detail
 {
     public long salesdetailcode { get; set; }

     public long salescode { get; set; }

     public long itemcode { get; set; }

    public string? itemname { get; set; }

     public int? days { get; set; }

     public int? prescribeddays { get; set; }

     public int? no_of_days { get; set; }

     public string? duration { get; set; }

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

     public decimal soldqty { get; set; }

     public decimal packsize { get; set; } = 1;

     public long? warehousecode { get; set; }

     public string? tenantcode { get; set; }

     public Guid? queue_id { get; set; }
     public Guid? pr_det_id { get; set; }

    // Dosage (M, A, E, N)
    public string? dosage { get; set; }
    public string? morning { get; set; }
    public string? afternoon { get; set; }
    public string? evening { get; set; }
    public string? night { get; set; }
    public string? morning_dose { get; set; }
    public string? afternoon_dose { get; set; }
    public string? evening_dose { get; set; }
    public string? night_dose { get; set; }
    public int? bh_code { get; set; }
    public decimal? cntcode { get; set; }
 }
    public class sales_request
    {
        public sales_master master { get; set; } = new();
        public List<sales_detail> details { get; set; } = new();
        public List<Guid>? queue_ids { get; set; }   // pharmacy queue rows being billed in this sale
    }
