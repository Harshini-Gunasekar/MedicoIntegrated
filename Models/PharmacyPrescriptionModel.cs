public class PharmacyPrescriptionQueueRow
{
    public Guid queue_id { get; set; } = Guid.NewGuid();
    public Guid? pr_det_id { get; set; }
    public string? pr_code { get; set; }
    public Guid? sheet_id { get; set; }
    public Guid? op_id { get; set; }
    public Guid? ip_id { get; set; }
    public decimal custid { get; set; }

    public string drug_name { get; set; } = string.Empty;
    public long? matched_itemcode { get; set; }
    public string? matched_itemname { get; set; }

    public decimal? qty { get; set; }
    public string? morning { get; set; }
    public string? afternoon { get; set; }
    public string? evening { get; set; }
    public string? night { get; set; }
    public string? morning_dose { get; set; }
    public string? afternoon_dose { get; set; }
    public string? evening_dose { get; set; }
    public string? night_dose { get; set; }
    public string? dose_unit { get; set; }   // mg / ml / tab / etc.
    public bool before_food { get; set; }
    public bool after_food { get; set; }
    public int? days { get; set; }
    public int? no_of_days { get; set; }
    public int? prescribed_days { get; set; }
    public int? prescribeddays { get; set; }
    public int? feed_days { get; set; }
    public string? duration { get; set; }
    public string? route { get; set; }
    public string? notes { get; set; }

    public string status { get; set; } = "PENDING";
    public bool billed_status { get; set; } = false;
    public string? billno { get; set; }
    public long? salescode { get; set; }
    public DateTime? billed_date { get; set; }
    public string? tenant_code { get; set; }
    public DateTime created_at { get; set; } = DateTime.UtcNow;
    public DateTime updated_at { get; set; } = DateTime.UtcNow;
}

public class ReceivePrescriptionRequest
{
    public decimal custid { get; set; }
    public string? op_id { get; set; }
    public string? ip_id { get; set; }
    public string? sheet_id { get; set; }
    public string? pr_code { get; set; }
    public List<PrescriptionQueueItem> items { get; set; } = new();
}

public class PrescriptionQueueItem
{
    public string? pr_det_id { get; set; }
    public string drug_name { get; set; } = string.Empty;
    public decimal qty { get; set; }
    public string? morning { get; set; }
    public string? afternoon { get; set; }
    public string? evening { get; set; }
    public string? night { get; set; }
    public string? morning_dose { get; set; }
    public string? afternoon_dose { get; set; }
    public string? evening_dose { get; set; }
    public string? night_dose { get; set; }
    public string? dose_unit { get; set; }   // mg / ml / tab / etc.
    public bool before_food { get; set; }
    public bool after_food { get; set; }
    public int? days { get; set; }
    public int? no_of_days { get; set; }
    public int? prescribed_days { get; set; }
    public int? prescribeddays { get; set; }
    public int? feed_days { get; set; }
    public string? duration { get; set; }
    public string? route { get; set; }
    public string? notes { get; set; }
}
public class MatchQueueItemRequest
{
    public Guid queue_id { get; set; }
    public long itemcode { get; set; }
}

public class DispenseQueueItemRequest
{
    public Guid queue_id { get; set; }
}
public class PharmacyQueueGroup
{
    public decimal custid { get; set; }
    public string? pr_code { get; set; }
    public Guid? sheet_id { get; set; }
    public Guid? op_id { get; set; }
    public Guid? ip_id { get; set; }
    public bool all_billed { get; set; } = false;
    public int unbilled_count { get; set; } = 0;
    public List<PharmacyPrescriptionQueueRow> items { get; set; } = new();
}

public class UpdateQueueStatusRequest
{
    public Guid queue_id { get; set; }
    public string status { get; set; } = string.Empty;   // PENDING / MATCHED / DISPENSED / CANCELLED
}

public class MarkBilledQueueRequest
{
    public List<Guid> queue_ids { get; set; } = new();
    public long? salescode { get; set; }
    public string? billno { get; set; }
}

public class MarkQueueBilledRequest
{
    public List<Guid> queue_ids { get; set; } = new();
    public long? salescode { get; set; }
    public string? billno { get; set; }
}

public class UnbillQueueRequest
{
    public List<Guid> queue_ids { get; set; } = new();
}
