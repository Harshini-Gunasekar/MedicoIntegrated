  public class sales_return_lookup_request
   {
       public long itemcode { get; set; }

       public string? batchno { get; set; }   // Optional
   }
   public class sales_return_lookup_result
   {
       public long salesdetailcode { get; set; }

       public long salescode { get; set; }

       public long itemcode { get; set; }

       public string? itemname { get; set; }

       public string? batchno { get; set; }

public long? customercode { get; set; }

     public string? customername { get; set; }

       public string? patientid { get; set; }

       public string? patientname { get; set; }

       public decimal rate { get; set; }

       public decimal deliveredqty { get; set; }

       public decimal returnedqty { get; set; }

       public decimal availableqty { get; set; }

       public decimal packsize { get; set; }

       public long? warehousecode { get; set; }
   }
   public class sales_return_request
   {
       public long salesdetailcode { get; set; }

       public long salescode { get; set; }

       public long itemcode { get; set; }

       public long? customercode { get; set; }

       public string? batchno { get; set; }

       public decimal returnqty { get; set; }

       public decimal packsize { get; set; }

       public long? warehousecode { get; set; }

       public string? remarks { get; set; }

       public long? usercode { get; set; }

       public string? tenantcode { get; set; }
   }
  
   public class sales_return_master
   {
       
       public long salesreturncode { get; set; }

       public long salesdetailcode { get; set; }

       public long salescode { get; set; }

       public long itemcode { get; set; }

       public long? customercode { get; set; }

       public string? batchno { get; set; }

       public decimal returnqty { get; set; }

       public decimal packsize { get; set; }

       public decimal totalqty { get; set; }

       public decimal rate { get; set; }

       public decimal amount { get; set; }

       public long? warehousecode { get; set; }

       public string? remarks { get; set; }

       public bool isactive { get; set; }

       public bool deleted { get; set; }

       public DateTime createddate { get; set; }

       public long? usercode { get; set; }

       public string? tenantcode { get; set; }

       public string? customername { get; set; }

       public string? patientname { get; set; }

       public string? patientid { get; set; }

       public string? itemname { get; set; }

       public string? warehousename { get; set; }
   }
