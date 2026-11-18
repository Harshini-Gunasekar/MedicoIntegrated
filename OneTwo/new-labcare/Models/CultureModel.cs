using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using Dapper.Contrib.Extensions;

namespace Lims.Components.Model
{
    public class CultureSaveRequestDto
    {
        public LabCultureMaster culture { get; set; } = new();
        public int? tcode { get; set; }
        public List<CultureIsolationRowDto>? isolationRows { get; set; }
    }

    public class CultureEntryGetDto
    {
        public CultureEntryWithNamesDto? culture { get; set; }
        public List<CultureIsolationRowDto>? isolationRows { get; set; }
        public List<LabCultureDetails>? cultureDetails { get; set; }
    }

    public class CultureOrganismDto
    {
        public int cocode { get; set; }
        public int orderno { get; set; }
        public string? shortname { get; set; }
        public string? name { get; set; }
        public bool? isnogrowth { get; set; }
    }

    public class CultureBasicInfoDto
    {
        public string? customername { get; set; }
        public int? billno { get; set; }
        public long? custid { get; set; }
        public long? dcode { get; set; }
        public string? referralname { get; set; }
    }

    public class AuthorizeCultureDto
    {
        public Guid resultcultureguid { get; set; } 
        public int? authorizedby { get; set; }
        public string? reportstatus { get; set; }
    }

    public class LoadVitekToIsolationDto
    {
        public Guid resultcultureguid { get; set; }
        public string isolationtarget { get; set; } = "A";
        public string? vitekorganism { get; set; }
    }
    public class CultureTestListDto
    {
        public Guid? resultcultureguid { get; set; }
        public int? tcode { get; set; }
        public string? testname { get; set; }
        public bool? isauthorized { get; set; }
        public string? reportstatus { get; set; }
        public string? statuslabel { get; set; }  // "Final" | "Authorized" | "Preliminary" | "Pending"
        public int? gcode { get; set; }
        public int? gCode { get => gcode; set => gcode = value; }
        public int? GCode { get => gcode; set => gcode = value; }
    }
    public class CultureReportIsolationDto
    {
        public string? antibiotic { get; set; }
        public string? columnname { get; set; }
        public int? actualsno { get; set; }
        public int? sno { get; set; }
        public int? abcode { get; set; }
        public string? isolation_a { get; set; }
        public string? isolation_b { get; set; }
        public string? isolation_c { get; set; }
        public string? mma { get; set; }
        public string? mmb { get; set; }
        public string? mmc { get; set; }
        public string? diskcontenta { get; set; }
        public string? diskcontentb { get; set; }
        public string? diskcontentc { get; set; }
        public string? reportstatus { get; set; }
    }

    // ─────────────────────────────────────────────────────────────
    // DTO: DataSet3 query result
    // ─────────────────────────────────────────────────────────────
    public class CultureReportDataSet3Dto
    {
        public int? requestsno { get; set; }
        public string? requestbarcode { get; set; }
        public string? requestsnoprint { get; set; }
        public DateTime? requestdatetime { get; set; }
        public DateTime? requesteddatetime { get; set; }
        public DateTime? collecteddatetime { get; set; }
        public string? name { get; set; }
        public string? gender { get; set; }
        public string? dateofbirth { get; set; }
        public string? address { get; set; }
        public string? mobileno { get; set; }
        public string? roomno { get; set; }
        public string? hospitalid { get; set; }
        public string? pathologyno { get; set; }
        public int? alteredbhcode { get; set; }
        public int? seconddcode { get; set; }
        public string? seconddoctorname { get; set; }
        public double? requestamount { get; set; }
        public double? discountper { get; set; }
        public double? discountamount { get; set; }
        public double? totalamount { get; set; }
        public double? paidamount { get; set; }
        public string? description { get; set; }
        public string? requestguid { get; set; }
        public string? custcode { get; set; }
        public string? email { get; set; }
        public string? doctor { get; set; }
        public int? dcode { get; set; }
        public int? doctorcode { get; set; }
        public string? qualification { get; set; }
        public string? qualificationname { get; set; }
        public string? initial { get; set; }
        public string? nametitle { get; set; }
        public string? doctorareaname { get; set; }
        public string? doctorcityname { get; set; }
        public string? testname { get; set; }
        public int? tcode { get; set; }
        public int? testsno { get; set; }
        public int? resultculturesno { get; set; }
        public DateTime? samplereceiveddate { get; set; }
        public string? sample { get; set; }
        public string? colonycount { get; set; }
        public string? organismsgrown { get; set; }
        public string? organismsgrownb { get; set; }
        public string? organismsgrownc { get; set; }
        public string? reportingmethod { get; set; }
        public string? puscells { get; set; }
        public string? bacteria { get; set; }
        public DateTime? culturereporteddate { get; set; }
        public string? gramsstaining { get; set; }
        public string? smearafb { get; set; }
        public string? growthgrade { get; set; }
        public string? diagnosis { get; set; }
        public int? cfcode { get; set; }
        public string? footertext { get; set; }
        public bool? isgrowth { get; set; }
        public bool? isisolationa { get; set; }
        public bool? isisolationb { get; set; }
        public bool? isisolationc { get; set; }
        public string? colonycountb { get; set; }
        public string? colonycountc { get; set; }
        public string? commentsa { get; set; }
        public string? commentsb { get; set; }
        public string? commentsc { get; set; }
        public string? comments { get; set; }
        public string? reportstatus { get; set; }
        public bool? isauthorized { get; set; }
        public string? sensitiveantibioticname { get; set; }
        public string? sensitiveantibioticquantity { get; set; }
        public string? msensitiveantibioticname { get; set; }
        public string? msensitiveantibioticquantity { get; set; }
        public string? resistentantibioticname { get; set; }
        public string? resistentantibioticquantity { get; set; }
        public string? sensitiveantibioticnameb { get; set; }
        public string? sensitiveantibioticquantityb { get; set; }
        public string? msensitiveantibioticnameb { get; set; }
        public string? msensitiveantibioticquantityb { get; set; }
        public string? resistentantibioticnameb { get; set; }
        public string? resistentantibioticquantityb { get; set; }
        public string? sensitiveantibioticnamec { get; set; }
        public string? sensitiveantibioticquantityc { get; set; }
        public string? msensitiveantibioticnamec { get; set; }
        public string? msensitiveantibioticquantityc { get; set; }
        public string? resistentantibioticnamec { get; set; }
        public string? resistentantibioticquantityc { get; set; }
        public int? authorizedbycode { get; set; }
        public string? authorizedbyname { get; set; }
        public byte[]? authorizedbysignatureimage { get; set; }
        public int? enteredbycode { get; set; }
        public byte[]? enteredbysignatureimage { get; set; }
        public int? defaultusercode { get; set; }
        public string? defaultusername { get; set; }
        public byte[]? defaultusersignature { get; set; }
        public byte[]? signatureimage { get; set; }
        public string? reporttitle { get; set; }
        public string? expr1 { get; set; }
        public string? expr2 { get; set; }
        public string? ctitle { get; set; }
        public string? cintial { get; set; }
        public string? cname { get; set; }
        public string? reference { get; set; }
        public string? reporting { get; set; }
        public string? cityname { get; set; }
        public string? areapincode { get; set; }
        public string? docaddress { get; set; }
        public string? groupname { get; set; }
        public string? samplereceived { get; set; }
        public string? areaname { get; set; }
        public string? enteredbydesignation { get; set; }
        public string? authorizedbydesignation { get; set; }
        public string? defaultuserdesignation { get; set; }
        public string? onlinecode { get; set; }
        public int? typesno { get; set; }
    }
    public class RoutineReportDto
    {
        public string? ClientID { get; set; }
        public string? ReportName { get; set; }
        public string? RequestGUID { get; set; }
        public string? DataSource { get; set; }
        public string? TCode { get; set; }
        public List<ReportParameterDto> Parameters { get; set; } = new();
    }

    public class ReportParameterDto
    {
        public string? Name { get; set; }
        public List<string>? Values { get; set; }
    }

    public class RoutineReportMode
    {
        public string? ClientID { get; set; }
        public string? ReportName { get; set; }
        public string? RequestGUID { get; set; }
        public string? DataSource { get; set; }
        public string? TCode { get; set; }
        public List<ReportParameterDto> Parameters { get; set; } = new();
    }
    public class TestCodeNameDto
    {
        public int TCode { get; set; }
        public string TestName { get; set; } = string.Empty;
        public int gcode { get; set; }  
        public int gCode { get => gcode; set => gcode = value; }
        public int GCode { get => gcode; set => gcode = value; }
    }
     public class LabCultureMaster
    {
        [Dapper.Contrib.Extensions.ExplicitKey]

        public Guid? resultcultureguid { get; set; }

        public int? resultculturesno { get; set; }
        public string? resultculturebarcode { get; set; }
        public string? resultcultureconvertedbarcode { get; set; }
        public DateTime? resultculturedatetime { get; set; }
        public Guid? requestguid { get; set; }
        public decimal? recode { get; set; }
        public string? sample { get; set; }
        public DateTime? samplereceiveddate { get; set; }
        public string? colonycount { get; set; }
        public string? organismsgrown { get; set; }
        public string? puscells { get; set; }
        public string? bacteria { get; set; }
        public DateTime? culturereporteddate { get; set; }
        public int? rtmcode { get; set; }
        public string? reportingmethod { get; set; }
        public int? cfcode { get; set; }
        public string? footertext { get; set; }
        public string? gramsstaining { get; set; }
        public string? resultsms { get; set; }
        public string? description { get; set; }
        public bool deleted { get; set; } = false;
        public int? usercode { get; set; }
        public int? computercode { get; set; }
        public DateTime? entereddate { get; set; }
        public DateTime? ibsdate { get; set; }
        public int? resultenteredby { get; set; }
        public int? resultauthorizedby { get; set; }
        public bool? isauthorized { get; set; }
        public string? reportstatus { get; set; }
        public bool? isgrowth { get; set; }
        public string? organismsgrownb { get; set; }
        public string? organismsgrownc { get; set; }
        public bool? isisolationa { get; set; }
        public bool? isisolationb { get; set; }
        public bool? isisolationc { get; set; }
        public string? growthgrade { get; set; }
        public string? diagnosis { get; set; }
        public string? colonycountb { get; set; }
        public string? colonycountc { get; set; }
        public string? smearafb { get; set; }
        public string? comments { get; set; }
        public string? commentsa { get; set; }
        public string? commentsb { get; set; }
        public string? commentsc { get; set; }
        public string? tenant_code { get; set; }
    }
    [Dapper.Contrib.Extensions.Table("lab_culture_details")]
    public class LabCultureDetails
    {
        [Dapper.Contrib.Extensions.Key]
        public Guid lcdid { get; set; }
        public Guid? resultcultureguid { get; set; }
        public string tenant_code { get; set; } = string.Empty;
        public int? tcode { get; set; }
        public int? typesno { get; set; }
        public string? sensitiveantibioticname { get; set; }
        public string? sensitiveantibioticquantity { get; set; }
        public string? msensitiveantibioticname { get; set; }
        public string? msensitiveantibioticquantity { get; set; }
        public string? resistentantibioticname { get; set; }
        public string? resistentantibioticquantity { get; set; }

        // ── These are INTEGER in the DB, not text ──
        public int? sensitivestylecode { get; set; }
        public int? sensitivequanitystylecode { get; set; }
        public int? msensitivestylecode { get; set; }
        public int? msensitivequantitystylecode { get; set; }
        public int? resistentstylecode { get; set; }
        public int? resistentquantitystylecode { get; set; }

        public string? reportstatus { get; set; }
        public string? isolation1 { get; set; }
    }

    [Dapper.Contrib.Extensions.Table("lab_culture_isolation")]
    public class CultureIsolationRowDto
    {
        [Dapper.Contrib.Extensions.Key]
        public Guid lcdiiid { get; set; }   // PRIMARY KEY

        public Guid? resultcultureguid { get; set; }   // FK → lab_culture_master

        public string tenant_code { get; set; } = string.Empty;

        public int? tcode { get; set; }

        public string? columnname { get; set; }

        public int? actualsno { get; set; }

        public int? sno { get; set; }

        public int? abcode { get; set; }

        public string? isolation_a { get; set; }

        public string? isolation_b { get; set; }

        public string? isolation_c { get; set; }

        public string? mma1 { get; set; }

        public string? mma2 { get; set; }

        public string? mma3 { get; set; }

        public string? diskcontenta { get; set; }

        public string? diskcontentb { get; set; }

        public string? diskcontentc { get; set; }

        public string? reportstatus { get; set; }
    }
    [Dapper.Contrib.Extensions.Table("lab_result_culture_antibiotics")]
    public class LabResultCultureAntibioticsModel
    {
        [Dapper.Contrib.Extensions.Key]
        public Guid lrcaid { get; set; }
        public Guid? testresultid { get; set; }
        public Guid lcdiiid { get; set; }
        public string? columnname { get; set; }
        public int? actualsno { get; set; }
        public int? sno { get; set; }
        public int? abcode { get; set; }

        // ── Isolation A ──
        public string? isolation_a { get; set; }
        public string? diskcontenta { get; set; }
        public string? mma { get; set; }

        public string? reportstatus { get; set; }
        public string? tenant_code { get; set; }
    }
    [Dapper.Contrib.Extensions.Table("lab_result_culture_master")]
    public class LabResultCultureMaster
    {
        [Dapper.Contrib.Extensions.Key]
        public Guid lrcmid { get; set; }
        public Guid testresultid { get; set; }
        public string tenant_code { get; set; } = string.Empty;
        public bool? isgrowth { get; set; } // Matches SQL
        public string? reportingmethod { get; set; }
        public string? sample { get; set; }
        public string? directsmear { get; set; }
        public string? growthgrade { get; set; }
        public DateTime? samplereceiveddate { get; set; }
        public DateTime? culturereporteddate { get; set; }
        public string? smearafb { get; set; }
        public string? puscells { get; set; }
        public string? gramsstaining { get; set; }
        public string? diagnosis { get; set; }
        public string? organismsgrown { get; set; }
        public string? organismsgrownb { get; set; }
        public string? organismsgrownc { get; set; }
        public bool? islsolationa { get; set; }
        public bool? islsolationb { get; set; }
        public bool? islsolationc { get; set; }
        public string? colonycount { get; set; }
        public string? colonycountb { get; set; }
        public string? colonycountc { get; set; }
        public string? comments { get; set; }
        public string? commentsa { get; set; }
        public string? commentsb { get; set; }
        public string? commentsc { get; set; }
        public string? bacteria { get; set; }
        public string? note { get; set; }
        public string? notemessage { get; set; }
        public string? bottomcomments { get; set; }
        public string? reportstatus { get; set; }
        public bool? deleted { get; set; }
        public DateTime? entereddate { get; set; }
    }

    public class CultureEntryWithNamesDto : LabCultureMaster
    {
        public string? resultenteredbyname { get; set; }
        public string? resultauthorizedbyname { get; set; }
    }

}