using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace LabCare.Models
{
    [Table("lab_request_master")]
    public class lab_request_master
    {
        [ExplicitKey]  // ✅ Guid key - not auto-increment
        public Guid? requestguid { get; set; }

        public int? requestsno { get; set; }
        public string? requestsnoprint { get; set; }
        public string? requestbarcode { get; set; }
        public string? requestconvertedbarcode { get; set; }
        public DateTimeOffset? requestdatetime { get; set; }
        public DateTimeOffset? requesteddatetime { get; set; }
        public DateTimeOffset? collecteddatetime { get; set; }
        public long? bncode { get; set; }
        public long? cntcode { get; set; }
        public Guid? cnttid { get; set; }
        public long? tmcode { get; set; }
        public long? custid { get; set; }
        public long? dcode { get; set; }
        public long? ricode { get; set; }
        public long? packcode { get; set; }
        public long? ftcode { get; set; }
        public long? ctcode { get; set; }
        public long? dlcode { get; set; }
        public long? pmcode { get; set; }
        public long? icode { get; set; }
        public string? name { get; set; }
        public string? gender { get; set; }
        public string? dateofbirth { get; set; }
        public string? ageyears { get; set; }
        public string? agemonths { get; set; }
        public string? agedays { get; set; }
        public string? address { get; set; }
        public int? areacode { get; set; }
        public string? mobileno { get; set; }
        public double? requestamount { get; set; }
        public double? discountper { get; set; }
        public double? discountamount { get; set; }
        public double? totalamount { get; set; }
        public double? paidamount { get; set; }
        public double? paidviareceipt { get; set; }
        public string? billtype { get; set; }
        public string? orderguid { get; set; }
        public double? ourdispercentage { get; set; }
        public double? ourdiscount { get; set; }
        public bool? ourdiscountisper { get; set; }
        public bool? referaldiscountisper { get; set; }
        public bool? isbillhavingresult { get; set; }
        public bool? isinvestigation { get; set; }
        public bool? isinvestigationstatus { get; set; }
        public bool? isinvestigationpartial { get; set; }
        public bool? deliverystatus { get; set; }
        public bool? requeststatus { get; set; }
        public bool? resultstatus { get; set; }
        public bool? ishavinglockresult { get; set; }
        public string? onlinecode { get; set; }
        public bool? onlineviewstatus { get; set; }
        public string? onlinepassword { get; set; }
        public int? enteredbhcode { get; set; }
        public int? alteredbhcode { get; set; }
        public string? roomno { get; set; }
        public string? hospitalid { get; set; }
        public bool? deleted { get; set; }
        public bool? isverified { get; set; }
        public int? usercode { get; set; }
        public int? computercode { get; set; }
        public DateTimeOffset? entereddate { get; set; }
        public DateTimeOffset? ibsdate { get; set; }
        public double? specialdiscount { get; set; }
        public double? newamount { get; set; }
        public bool? iscashbill { get; set; }
        public bool? iscreditbill { get; set; }
        public string? fronthospitalid { get; set; }
        public string? fronthospitalpatientid { get; set; }
        public string? hospitalpatientid { get; set; }
        public bool? ispricechanged { get; set; }
        public DateTimeOffset? lastpaymenton { get; set; }
        public int? seconddcode { get; set; }
        public string? seconddoctorname { get; set; }
        public bool? ishomecollection { get; set; }
        public int? homecollectionareacode { get; set; }
        public double? homecollectioncharge { get; set; }
        public string? concessionreason { get; set; }
        public bool? isposted { get; set; }
        public int? billbncode { get; set; }
        public int? billno { get; set; }
        public string? billnoprint { get; set; }
        public string? pathologyno { get; set; }
        public double? dueconcession { get; set; }
        public bool? isbillissued { get; set; }
        public bool? isinventoryposted { get; set; }
        public bool? istraced { get; set; }
        public bool? isinvestigationauthorized1 { get; set; }
        public bool? isinvestigationauthorized2 { get; set; }
        public bool? isinvestigationprinted1 { get; set; }
        public double? pmc1 { get; set; }
        public double? pmc2 { get; set; }
        public double? pmc3 { get; set; }
        public bool? iscollectionposted { get; set; }
        public bool? issendsms { get; set; }
        public double? refundamount { get; set; }
        public string? card_refno { get; set; }
        public string? bank_app { get; set; }

        public int? doc1_dmid { get; set; }      // FK → document_master.dmid
        public string? doc1_dmvalue { get; set; } // e.g. "6190 4411 2233"
        public string? doc1_path { get; set; }

        public int? doc2_dmid { get; set; }      // FK → document_master.dmid
        public string? doc2_dmvalue { get; set; }
        public string? doc2_path { get; set; }
        public string? tenant_code { get; set; }
        public string? careoftitile { get; set; }
        public string? careofinitial { get; set; }
        public string? careof { get; set; }
    }

    [Table("lab_request_details")]
    public class lab_request_details
    {
        [ExplicitKey]
        public Guid requestdetailsid { get; set; }
        public Guid? requestguid { get; set; }
        public int? testsno { get; set; }
        public int? packcode { get; set; }
        public long? tcode { get; set; }
        public double? testamount { get; set; }
        public double? discount { get; set; }
        public double? collection { get; set; }

        // ✅ Replaces: isinvestigation, isculture, istextreport
        public int ttid { get; set; }

        [Dapper.Contrib.Extensions.Computed]
        public bool isinvestigation { get; set; }
        [Dapper.Contrib.Extensions.Computed]
        public bool isculture { get; set; }
        [Dapper.Contrib.Extensions.Computed]
        public bool istextreport { get; set; }

        public bool? istesthavingresult { get; set; }
        public bool? resultstatus { get; set; }
        public bool? islockresult { get; set; }
        public bool? isprinted { get; set; }
        public bool? isauthorized1 { get; set; }
        public double? standardprice { get; set; }
        public bool? ispack { get; set; }
        public int? resultenteredby { get; set; }
        public int? resultauthorizedby { get; set; }
        public bool? isauthorized2 { get; set; }
        public int? resultauthorizedby2 { get; set; }
        public bool? isworklistprinted { get; set; }
        public int? qty { get; set; }
        public double? testrate { get; set; }
        public double? refundamount { get; set; }
        public DateTimeOffset? resultentereddate { get; set; }
        public DateTimeOffset? firstauthorizedate { get; set; }
        public DateTimeOffset? secondauthorizedate { get; set; }
        public string? tenant_code { get; set; }
    }

    [Table("lab_request_patientdetails")]
    public class lab_request_patientdetails
    {
        [ExplicitKey] public Guid lrpdid { get; set; }
        public Guid? requestguid { get; set; }
        public int? sno { get; set; }
        public int pdcode { get; set; }
        public string pdvalue { get; set; }
        public string? tenant_code { get; set; }
    }
}
