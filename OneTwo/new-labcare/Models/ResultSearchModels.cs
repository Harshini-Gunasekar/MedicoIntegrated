using System;
using System.Collections.Generic;

namespace LabCare.Models
{
    public class ViewResultSearch
    {
        public int requestsno { get; set; }
        public DateTimeOffset requestdatetime { get; set; }
        public string? custcode { get; set; }
        public string? name { get; set; }
        public string? age { get; set; } // Preserved for UI compatibility
        public string? sex { get; set; } // Preserved for UI compatibility
        public string? doctor { get; set; }

        public decimal requestamount { get; set; }
        public decimal discountper { get; set; }
        public decimal discountamount { get; set; }
        public decimal totalamount { get; set; }
        public decimal paidamount { get; set; }
        public decimal balance { get; set; }

        public Guid requestguid { get; set; }
        public string? testshortname { get; set; } // Preserved for UI compatibility

        // Request-level type flags (same value on every test row for same request)
        public bool? isinvestigation { get; set; }
        public bool? isculture { get; set; }
        public bool? istextreport { get; set; }

        // Per-test columns (vary row to row)
        public string? tcode { get; set; }
        public int? ttid { get; set; }        // 1=Investigation, 2=Culture, 4=TextReport
        public bool? resultstatus { get; set; }
        public bool? isprinted { get; set; }
        public bool? isauthorized1 { get; set; }
        public bool? isauthorized2 { get; set; }
        public bool? isworklistprinted { get; set; }

        public int? gcode { get; set; }
        public string? groupname { get; set; }
        public string? testname { get; set; }

        public bool? isscan { get; set; }
        public bool? islab { get; set; }

        public int? enteredbhcode { get; set; }
        public int? alteredbhcode { get; set; }
        public long? bncode { get; set; }

        public int? billno { get; set; }
        public string? billnoprint { get; set; }
        public int? billbncode { get; set; }

        public int? dcode { get; set; }
        public string? mobile { get; set; }
        public string? doctorfullname { get; set; }

        public bool? requestresultstatus { get; set; }
        public bool? isinvestigationauthorized1 { get; set; }
        public bool? isinvestigationauthorized2 { get; set; }
        public bool? isinvestigationprinted1 { get; set; }

        public string? pathologyno { get; set; }
        public string? onlinecode { get; set; }
        public string? onlinepassword { get; set; }
        public string? isothers { get; set; }
        public string? hospitalid { get; set; }
        public string? hospitalpatientid { get; set; }
        public bool? issendsms { get; set; }

        public string? address { get; set; }
        public string? area { get; set; }
        public string? city { get; set; }
        public string? zipcode { get; set; }
        public string? state { get; set; }
        public string? tenant_code { get; set; }
    }

    public class Branch_Master
    {
        public int bhcode { get; set; }
        public string? name { get; set; }
    }

    public class Doctor_Master
    {
        public int dcode { get; set; }
        public string? name { get; set; }
    }

    public class Test_Master
    {
        public int tcode { get; set; }
        public string name { get; set; } = "";
    }

    public class BillRequest
    {
        public LIMS_Backend.Model.CustomerModel? customer { get; set; }
        public LIMS_Backend.Model.lab_request_master? Request_Master { get; set; }
        public List<LIMS_Backend.Model.lab_request_details>? Request_Details { get; set; }
    }
}
