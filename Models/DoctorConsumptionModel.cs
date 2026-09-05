public class consumption_master
{
    public long consumptioncode { get; set; }
    public string? consumptionno { get; set; }
    public DateTime consumptiondate { get; set; }

    public long roomwarehousecode { get; set; }   // must be a warehouse_master row with isdoctor_room = true
    public string? departmentname { get; set; }
    public string? doctorname { get; set; }
    public string? remarks { get; set; }

    public decimal totalitems { get; set; }
    public decimal totalqty { get; set; }
    public decimal totalvalue { get; set; }

    public bool isactive { get; set; } = true;
    public bool deleted { get; set; } = false;
    public DateTime createddate { get; set; }
    public DateTime? modifieddate { get; set; }
    public long? usercode { get; set; }
    public string? tenantcode { get; set; }
    public string? branchcode { get; set; }
}

public class consumption_detail
{
    public long consumptiondetailcode { get; set; }
    public long consumptioncode { get; set; }

    public long stockcode { get; set; }     // exact batch/stock row selected by the user
    public long itemcode { get; set; }
    public string? batchno { get; set; }

    public decimal availableqty { get; set; }   // system-filled, snapshot before deduction
    public decimal consumedqty { get; set; }    // user input
    public decimal unitcost { get; set; }       // system-filled
    public decimal stockvalue { get; set; }     // system-filled

    public string? remarks { get; set; }
    public string? tenantcode { get; set; }
}

public class consumption_request
{
    public consumption_master master { get; set; } = new();
    public List<consumption_detail> details { get; set; } = new();
}
