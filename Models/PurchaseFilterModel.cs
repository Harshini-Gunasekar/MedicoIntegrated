public class purchase_filter_result
{
    public long purchasecode { get; set; }
    public string? billno { get; set; }
    public DateTime billdate { get; set; }
    public string? invoiceno { get; set; }
    public DateTime? invoicedate { get; set; }
    public long? vendorcode { get; set; }
    public string? vendorname { get; set; }
    public decimal grossamount { get; set; }
    public decimal totalamount { get; set; }
    public decimal taxamount { get; set; }
    public decimal netamount { get; set; }
    public string? paymentmode { get; set; }
    public string? paymentstatus { get; set; }
    public DateTime createddate { get; set; }
}

// ─── PURCHASE BULK STATUS UPDATE ──────────────────────────────────────────────
public class purchase_bulk_status_request
{
    public List<long> purchasecodes { get; set; } = new();
    public string paymentstatus { get; set; } = string.Empty;
    public long? usercode { get; set; }
}  