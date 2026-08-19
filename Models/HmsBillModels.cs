using System;
using System.Collections.Generic;

namespace Booking.Models
{
    public class HmsBillModel
    {
        public string? requestguid { get; set; }
        public string? op_id { get; set; }
        public Guid? ip_id { get; set; }
        public string? sheet_id { get; set; }
        public decimal custid { get; set; }
        public string? patient_name { get; set; }
        public string? gender { get; set; }
        public string? dateofbirth { get; set; } // YYYY-MM-DD
        public string? ageyears { get; set; }
        public string? agemonths { get; set; }
        public string? agedays { get; set; }
        public string? mobileno { get; set; }
        public string? address { get; set; }
        public int? areacode { get; set; }
        public int dcode { get; set; }
        public int consultantdcode { get; set; }
        public int enteredbhcode { get; set; } = 1;
        public int cntcode { get; set; } = 2;
        public int usercode { get; set; } = 10;
        public int computercode { get; set; } = 1;
        public int ftcode { get; set; } = 1;
        public int pmcode { get; set; } = 1;
        public int ctcode { get; set; } = 2;
        public string? ricode { get; set; }
        public decimal discountper { get; set; }
        public decimal discountamount { get; set; }
        public decimal specialdiscount { get; set; }
        public decimal ourdispercentage { get; set; }
        public decimal ourdiscount { get; set; }
        public decimal paidamount { get; set; }
        public double? pmc1 { get; set; } // Paymode code 1
        public double? pmc2 { get; set; } // Paymode code 2
        public double? pmc3 { get; set; } // Paymode code 3
        public double? pmc1_amount { get; set; } // Paymode amount 1
        public double? pmc2_amount { get; set; } // Paymode amount 2
        public double? pmc3_amount { get; set; } // Paymode amount 3
        public string? collection_type { get; set; } = "CASH"; // CASH, CARD, GPAY, MULTI
        public bool iscashbill { get; set; } = true;
        public bool iscreditbill { get; set; } = false;
        public bool isinsurancepatient { get; set; } = false;
        public string? policyno { get; set; }
        public string? authorisationno { get; set; }
        public string? concessionreason { get; set; }
        public string? card_refno { get; set; }
        public string? bank_app { get; set; }
        public List<string>? unbilled_charge_ids { get; set; } = new();

        public List<HmsBillItemModel> items { get; set; } = new();
    }

    public class HmsBillItemModel
    {
        public int sno { get; set; }
        public string charge_type { get; set; } = "INVESTIGATION";
        public string? item_name { get; set; }
        public decimal tcode { get; set; }
        public string? item_ref_id { get; set; }
        public decimal unit_rate { get; set; }
        public decimal amount { get; set; }
        public decimal qty { get; set; } = 1;
        public decimal discount { get; set; } // value
        public decimal gst_per { get; set; }
        public int ttid { get; set; } = 1;
        public string? requestdetailsid { get; set; }
    }

    public class UnbilledChargeSummary
    {
        public string? unbilledid { get; set; }
        public string? entrytype { get; set; }
        public string? entryid { get; set; }
        public DateTime? chargedate { get; set; }
        public int? tcode { get; set; }
        public double? quantity { get; set; }
        public double? rate { get; set; }
        public double? amount { get; set; }
        public string? item_name { get; set; }
        public decimal? custid { get; set; }
        public string? opvisitid { get; set; }
        public Guid? ip_id { get; set; }
        public int? bedcode { get; set; }
        public double? discount { get; set; }
        public double? charityamount { get; set; }
        public bool? billedstatus { get; set; }
    }

    public class AddUnbilledConsultationRequest
    {
        public string op_id { get; set; } = string.Empty;
        public decimal custid { get; set; }
        public int? tcode { get; set; }
        public double rate { get; set; }
        public double amount { get; set; }
        public double quantity { get; set; } = 1;
    }

    public class UpdateUnbilledChargeRequest
    {
        public string unbilledid { get; set; } = string.Empty;
        public double? rate { get; set; }
        public double? quantity { get; set; }
        public double? amount { get; set; }
        public double? discount { get; set; }
        public double? charityamount { get; set; }
        public string? tenant_code { get; set; }

        [System.Text.Json.Serialization.JsonIgnore]
        public string? unbilled_id { get => unbilledid; set { if (!string.IsNullOrEmpty(value)) unbilledid = value; } }

        [System.Text.Json.Serialization.JsonIgnore]
        public double? qty { get => quantity; set { if (value.HasValue) quantity = value; } }

        [System.Text.Json.Serialization.JsonIgnore]
        public double? concession { get => discount; set { if (value.HasValue) discount = value; } }

        [System.Text.Json.Serialization.JsonIgnore]
        public double? charity { get => charityamount; set { if (value.HasValue) charityamount = value; } }

        [System.Text.Json.Serialization.JsonIgnore]
        public string? tenantcode { get => tenant_code; set { if (!string.IsNullOrEmpty(value)) tenant_code = value; } }

        public void SynchronizeAliases()
        {
        }
    }
}
