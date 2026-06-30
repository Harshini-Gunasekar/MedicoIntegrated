using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;
using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace LabCare.Models
{
    [Table("test_master")]
    public class test_master
    {
        [Dapper.Contrib.Extensions.Key]
        public long tcode { get; set; }
        public long gcode { get; set; }
        public long scode { get; set; }
        public long rtcode { get; set; }
        public long ucode { get; set; }
        public long rtmcode { get; set; }
        public int orderno { get; set; }
        public string name { get; set; } = string.Empty;
        public string shortname { get; set; } = string.Empty;
        public string qty { get; set; } = string.Empty;
        public double amount { get; set; }

        // ✅ Replaces: isculture, routine, outlab, textcontent
        public int ttid { get; set; }


        public bool lockresult { get; set; }
        public bool locksms { get; set; }
        public bool? deleted { get; set; }
        public int? usercode { get; set; }
        public int? computercode { get; set; }
        public DateTimeOffset? entereddate { get; set; }
        public DateTimeOffset? ibsdate { get; set; }
        public bool printinseparatepage { get; set; }
        public bool printgraphinreport { get; set; }
        public string? graphtype { get; set; }
        public long? cgcode { get; set; }
        public bool? istest { get; set; }
        public bool? ispackage { get; set; }
        public int? packcode { get; set; }
        public string? skycode { get; set; }
        public bool? iscontrast { get; set; }
        public bool? isnoic { get; set; }
        public bool? isdc { get; set; }
        public string? tenant_code { get; set; }
        public decimal? tax_rate { get; set; }
        public string? icd_code { get; set; }
        public string? footer { get; set; }
        public bool? is_escalation { get; set; }


        // ✅ Helper — no DB column needed, excluded from API serialization
        [Dapper.Contrib.Extensions.Computed]
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string? test_type_name { get; set; }
    }

    [Table("test_result_master")]
    public class test_result_master
    {
        [Dapper.Contrib.Extensions.Key]
        public long trcode { get; set; }
        public Guid testresultid { get; set; }
        public Guid? trguid { get; set; }
        public long slno { get; set; }
        public long tcode { get; set; }
        public int? fromtcode { get; set; }
        public string resulttype { get; set; } = string.Empty;
        public string? col2 { get; set; }
        public string? col3 { get; set; }
        public string? col4 { get; set; }
        public string? col5 { get; set; }
        public string? col6 { get; set; }
        public string? cellcontent { get; set; }
        public string? normalfemale { get; set; }
        public string? normalchild { get; set; }
        public int? stylecode { get; set; }
        public bool? sendsms { get; set; }
        public string? smsshortname { get; set; }
        public bool? deleted { get; set; }
        public int? usercode { get; set; }
        public int? computercode { get; set; }
        public DateTimeOffset? entereddate { get; set; }
        public DateTimeOffset? ibsdate { get; set; }
        public bool? printinseparatepage { get; set; }
        public bool? iscalculated { get; set; }
        public bool? isentered { get; set; }
        public string? calculatedformula { get; set; }
        public int? dstylecode { get; set; }
        public int? qstylecode { get; set; }
        public int? estylecode { get; set; }
        public int? ustylecode { get; set; }
        public int? nstylecode { get; set; }
        public Guid? fromtestresultid { get; set; }
        public string? skycode { get; set; }
        public bool? is_escalation { get; set; }
        public string? tenant_code { get; set; }
        public string? testimage { get; set; }
    }

    [Table("test_result_properties")]
    public class test_result_properties
    {
        [ExplicitKey]
        public Guid trpid { get; set; }
        public Guid? testresultid { get; set; }
        public string? resultvaluetype { get; set; }
        public int? defaultunitscode { get; set; }
        public Guid? fxtcode { get; set; }
        public Guid? defaultvalueforfxtype { get; set; }
        public string? defaultvalue { get; set; }
        public bool? simplenormalvalues { get; set; }
        public bool? detailednormalvalues { get; set; }
        public string? rangetype { get; set; }
        public double? fromnormalvalue { get; set; }
        public double? tonormalvalue { get; set; }
        public string? conclusionforhigher { get; set; }
        public string? conclusionforlower { get; set; }
        public bool? printfixedtextconclusioninreport { get; set; }
        public string? conclusionforfixedtext { get; set; }
        public bool? showagedbased { get; set; }
        public bool? printconclusioninreport { get; set; }
        public bool? printconclusioninbottom { get; set; }
        public bool? showalertonhigherlower { get; set; }
        public bool? isaddresult { get; set; }
        public bool? printunitsinnormalvalues { get; set; }
        public bool? printnormalvaluesatbottom { get; set; }
        public bool? printspecialfieldsatrightside { get; set; }
        public bool? groupvaluesbysex { get; set; }
        public bool? groupvaluesbyspecialfield { get; set; }
        public string? footermessage { get; set; }
        public int? rtmcode { get; set; }
        public bool? printresultonly { get; set; }
        public bool? isgraph { get; set; }
        public double? graphvalue { get; set; }
        public int? decimalvalue { get; set; }
        public int? scode { get; set; }
        public DateTimeOffset? entereddate { get; set; }
        public int? mccode { get; set; }
        public int? performedcount { get; set; }
        public bool? usedefault { get; set; }
        public Guid? normalvalueforfxtype { get; set; }
        public string? normalvalue { get; set; }
        public bool? isabnormal { get; set; }
        public string? criticallowtype { get; set; }
        public string? criticallowrange { get; set; }
        public string? criticalhightype { get; set; }
        public string? criticalhighrange { get; set; }
        public string? tenant_code { get; set; }
        public bool? istestimage { get; set; }
    }

    [Table("test_result_calculatedformula")]
    public class TestResultCalculatedformula
    {
        [ExplicitKey]
        public Guid trcfid { get; set; }
        public Guid? testresultid { get; set; }
        public string? sex { get; set; }
        public string? calculatedformula { get; set; }
        public DateTimeOffset? entereddate { get; set; }
        public int? mccode { get; set; }
        public int? performedcount { get; set; }
        public int? scode { get; set; }
        public string? tenant_code { get; set; }
    }

    [Table("test_result_detailednormalvalues")]
    public class test_result_detailednormalvalues
    {
        [ExplicitKey]
        public Guid trdnid { get; set; }
        public Guid? testresultid { get; set; }

        public int? sno { get; set; }       // row order

        // Age range
        public int? agefrom { get; set; }
        public string? agefromtype { get; set; }  // Days, Mths, Yrs
        public int? ageto { get; set; }
        public string? agetotype { get; set; }    // Days, Mths, Yrs
        public string? sex { get; set; }

        public string? rangetype { get; set; }    // Between, >, <, etc.
        public double? rangefrom { get; set; }
        public double? rangeto { get; set; }

        public Guid? specialconditioncode { get; set; }
        public string? agerangetype { get; set; } // label like "Adult Male"

        public DateTimeOffset? entereddate { get; set; }
        public int? mccode { get; set; }
        public int? performedcount { get; set; }
        public int? scode { get; set; }
        public string? tenant_code { get; set; }
    }

    [Table("test_result_textnormalvalues")]
    public class test_result_textnormalvalues
    {
        [ExplicitKey]
        public Guid trtid { get; set; }
        public Guid? testresultid { get; set; }
        public string? sex { get; set; }

        public string? normalvalue { get; set; }  // display text e.g. "> 40 mg/dL"

        public DateTimeOffset? entereddate { get; set; }
        public int? mccode { get; set; }
        public int? performedcount { get; set; }
        public int? scode { get; set; }
        public string? tenant_code { get; set; }
    }

    [Table("test_type_master")]
    public class test_type_master
    {
        [Key]
        public long ttid { get; set; }
        public string shortname { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public bool? deleted { get; set; } = false;
        public int? usercode { get; set; }
        public DateTimeOffset? entereddate { get; set; }
        public DateTimeOffset? ibsdate { get; set; }
        public string? tenant_code { get; set; }
    }

    public class SampleModel
    {
        public long scode { get; set; }
        public int orderno { get; set; }
        public string? shortname { get; set; }
        public string name { get; set; }
        public string? description { get; set; }
        public bool? deleted { get; set; } = false;
        public int usercode { get; set; }
        public int? computercode { get; set; }
        public DateTime? entereddate { get; set; }
        public DateTime? ibsdate { get; set; }
        public string? tenant_code { get; set; }
    }

    [Table("uom_master")]
    public class UomMaster
    {
        [Key]
        public long ucode { get; set; }
        public int orderno { get; set; }
        public string name { get; set; }
        public string shortname { get; set; }
        public int? decimalplaces { get; set; }
        public string? description { get; set; }
        public bool? deleted { get; set; } = false;
        public int usercode { get; set; }
        public int? computercode { get; set; }
        public DateTime? entereddate { get; set; }
        public DateTime? ibsdate { get; set; }
        public int? packsize { get; set; }
        public string? tenant_code { get; set; }
    }

    public class ReportingModel
    {
        public long recode { get; set; }
        public int orderno { get; set; }
        public string shortname { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public byte[] signatureimage { get; set; }
        public bool deleted { get; set; }
        public int usercode { get; set; }
        public int computercode { get; set; }
        public DateTime? entereddate { get; set; }
        public DateTime? ibsdate { get; set; }
        public string? tenant_code { get; set; }
    }

    [Table("report_method")]
    public class ReportMethodModel
    {
        [Key]
        [JsonProperty("rtmcode")]
        public long rtmcode { get; set; }

        [JsonProperty("orderno")]
        public int orderno { get; set; }

        [JsonProperty("shortname")]
        public string? shortname { get; set; }

        [JsonProperty("name")]
        public string? name { get; set; }

        [JsonProperty("durationtime")]
        public int durationtime { get; set; }

        [JsonProperty("duration")]
        public string? duration { get; set; }

        [JsonProperty("description")]
        public string? description { get; set; }

        [JsonProperty("footer")]
        public string? footer { get; set; }

        [JsonProperty("isculture")]
        public bool isculture { get; set; }

        [JsonProperty("deleted")]
        public bool deleted { get; set; } = false;

        [JsonProperty("usercode")]
        public int usercode { get; set; } = 1;

        [JsonProperty("computercode")]
        public int computercode { get; set; } = 1;

        [JsonProperty("entereddate")]
        public DateTime entereddate { get; set; }

        [JsonProperty("ibsdate")]
        public DateTime ibsdate { get; set; }

        [JsonProperty("tenant_code")]
        public string? tenant_code { get; set; }
    }

    [Table("branch_master")]
    public class BranchModel
    {
        [Key]
        public int bh_code { get; set; }
        public int order_no { get; set; }
        public string? short_name { get; set; }
        public string name { get; set; }
        public string? address { get; set; }
        public string? city { get; set; }
        public string? pincode { get; set; }
        public string? state { get; set; }
        public string? country { get; set; }
        public string? phone { get; set; }
        public string? mobile { get; set; }
        public string? fax { get; set; }
        public string? email { get; set; }
        public string? website { get; set; }
        public string? description { get; set; }
        public int? area_code { get; set; }
        public bool deleted { get; set; } = false;
        public int user_code { get; set; }
        public int computer_code { get; set; }
        public DateTime entered_date { get; set; }
        public DateTime ibs_date { get; set; }
        public bool? is_main_branch { get; set; }
        public bool? is_branch { get; set; }
        public bool? is_collection_centre { get; set; }
        public string? pharmacy_name { get; set; }
        public string? lab_name { get; set; }
        public string? tenant_code { get; set; }
        public int? ftcode { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public int? bhcode { get => bh_code; set => bh_code = value ?? 0; }
        [Newtonsoft.Json.JsonIgnore]
        public int orderno { get => order_no; set => order_no = value; }
        [Newtonsoft.Json.JsonIgnore]
        public string? shortname { get => short_name; set => short_name = value; }
        [Newtonsoft.Json.JsonIgnore]
        public int? areacode { get => area_code; set => area_code = value; }
        [Newtonsoft.Json.JsonIgnore]
        public DateTime? entereddate { get => entered_date; set => entered_date = value ?? DateTime.Now; }
        [Newtonsoft.Json.JsonIgnore]
        public DateTime? ibsdate { get => ibs_date; set => ibs_date = value ?? DateTime.Now; }
        [Newtonsoft.Json.JsonIgnore]
        public bool? ismainbranch { get => is_main_branch; set => is_main_branch = value; }
        [Newtonsoft.Json.JsonIgnore]
        public bool? isbranch { get => is_branch; set => is_branch = value; }
        [Newtonsoft.Json.JsonIgnore]
        public bool? iscollectioncentre { get => is_collection_centre; set => is_collection_centre = value; }
        [Newtonsoft.Json.JsonIgnore]
        public int? usercode { get => user_code; set => user_code = value ?? 0; }
    }

    // ─── DTOs ─────────────────────────────────────────────────────────────────

    public class TestResultRowDto
    {
        [JsonProperty("resultMaster")]
        public test_result_master? ResultMaster { get; set; }
        [JsonProperty("resultProperties")]
        public test_result_properties? ResultProperties { get; set; }
        [JsonProperty("calculatedFormulas")]
        public List<TestResultCalculatedformula>? CalculatedFormulas { get; set; }
        [JsonProperty("detailedNormalValues")]
        public List<test_result_detailednormalvalues>? DetailedNormalValues { get; set; }
        [JsonProperty("textNormalValues")]
        public List<test_result_textnormalvalues>? TextNormalValues { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public byte[]? testImageBytes { get; set; }
        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string? testImageName { get; set; }
    }

    public class TestInsertDto
    {
        [JsonProperty("tcode")]
        public long tcode { get; set; }

        [JsonProperty("totalResults")]
        public int totalResults { get; set; }

        [JsonProperty("testMaster")]
        public test_master TestMaster { get; set; } = new();

        [JsonProperty("resultRows")]
        public List<TestResultRowDto>? ResultRows { get; set; }
    }

    public class TestFetchDto
    {
        [JsonProperty("tcode")]
        public long TCode { get; set; }
        [JsonProperty("totalResults")]
        public int TotalResults { get; set; }
        [JsonProperty("testMaster")]
        public test_master? TestMaster { get; set; }
        [JsonProperty("results")]
        public List<TestResultRowDto>? Results { get; set; }
    }

    public class UserMasterWrapper
    {
        [JsonPropertyName("user")]
        [JsonProperty("user")]
        public SharedComponents.Rcl.Models.User_Master User { get; set; }
    }

    public class UserAuth
    {
        public int usercode { get; set; }
        public string? token { get; set; }
    }
}

namespace LIMS_Backend.Model
{
    [Table("machine_master")]
    public class MachineMaster
    {
        [Key]
        public int mccode { get; set; }

        public int orderno { get; set; }
        public string? shortname { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }

        public string? manufacturer { get; set; }
        public string? model { get; set; }
        public string? portnumber { get; set; }
        public double? baudrate { get; set; }
        public string? parity { get; set; }
        public int? stopbits { get; set; }
        public int? databits { get; set; }

        public bool? deleted { get; set; } = false;

        public int usercode { get; set; }
        public int computercode { get; set; }

        public DateTime? entereddate { get; set; }
        public DateTime? ibsdate { get; set; }

        public bool? opttcp { get; set; }
        public bool? optlmport { get; set; }
        public string? inputpath { get; set; }
        public string? outputpath { get; set; }
        public bool? optrs232 { get; set; }
        public bool? opttcpclient { get; set; }
        public bool? opttcpserver { get; set; }

        public string? tenant_code { get; set; }
    }

    [Table("group_master")]
    public class GroupModel
    {
        [Key]
        public long gcode { get; set; }

        public long? dcode { get; set; }
        public int orderno { get; set; }
        public string? name { get; set; }
        public string? shortname { get; set; }
        public string? description { get; set; }
        public string? footer { get; set; }
        public long? departmentcode { get; set; }
        public bool? isscan { get; set; }
        public bool? islab { get; set; }
        public bool deleted { get; set; } = false;
        public int usercode { get; set; } = 1;
        public int computercode { get; set; } = 1;
        public DateTime entereddate { get; set; }
        public DateTime ibsdate { get; set; }
        public bool? ispackage { get; set; }
        public bool? ischarges { get; set; }
        public string? skycode { get; set; }
        public bool? isprintbarcode { get; set; }
        public int? workorderno { get; set; }
        public string? tenant_code { get; set; }
    }
}
