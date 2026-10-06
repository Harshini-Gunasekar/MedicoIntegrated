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

    public string? patientname { get; set; }    // Patient Name

    public string? mobileno { get; set; }

    public string? address { get; set; }        // Patient Address

    public string? consultant { get; set; }     // Consultant Name

    public int? prescribeddays { get; set; }   // total days prescribed by doctor

    public int? converteddays { get; set; }

    public decimal grossamount { get; set; }

    public decimal discountpercentage { get; set; }

    public decimal discountamount { get; set; }

    public decimal taxamount { get; set; }

    public decimal roundoff { get; set; }

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

    public decimal? cntcode { get; set; } = 0;

    public int? bh_code { get; set; }

    public string? cnttid { get; set; }   // counter shift session id

    // Legacy / UI compatibility aliases (syncs automatically with database fields)
    [System.Text.Json.Serialization.JsonIgnore]
    public string? customername { get => patientname; set => patientname = value; }

    [System.Text.Json.Serialization.JsonIgnore]
    public int? days { get => prescribeddays; set => prescribeddays = value; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string? duration { get => prescribeddays?.ToString(); set { if (int.TryParse(value, out var d)) prescribeddays = d; } }
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
    public decimal soldqty { get; set; }

    public decimal returnedqty { get; set; }

    public int? days { get; set; }

    public long? warehousecode { get; set; }

    public string? tenantcode { get; set; }

    public string? morning_dose { get; set; }

    public string? afternoon_dose { get; set; }

    public string? evening_dose { get; set; }

    public string? night_dose { get; set; }

    // Client-side UI & Queue helper properties
    public string? itemname { get; set; }

    public decimal packsize { get; set; } = 1;

    public Guid? queue_id { get; set; }

    public Guid? pr_det_id { get; set; }

    public string? dosage { get; set; }

    public int? bh_code { get; set; }

    public decimal? cntcode { get; set; }

    // Aliases to ensure duplicate days & dosages are never desynchronized
    [System.Text.Json.Serialization.JsonIgnore]
    public int? prescribeddays { get => days; set => days = value; }

    [System.Text.Json.Serialization.JsonIgnore]
    public int? no_of_days { get => days; set => days = value; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string? duration { get => days?.ToString(); set { if (int.TryParse(value, out var d) && d > 0) days = d; } }

    [System.Text.Json.Serialization.JsonIgnore]
    public string? morning { get => morning_dose; set => morning_dose = value; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string? afternoon { get => afternoon_dose; set => afternoon_dose = value; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string? evening { get => evening_dose; set => evening_dose = value; }

    [System.Text.Json.Serialization.JsonIgnore]
    public string? night { get => night_dose; set => night_dose = value; }
}

public class sales_request
{
    public sales_master master { get; set; } = new();
    public List<sales_detail> details { get; set; } = new();
    public List<Guid>? queue_ids { get; set; }   // pharmacy queue rows being billed in this sale
}
