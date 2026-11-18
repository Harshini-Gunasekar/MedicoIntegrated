namespace ReportingServer.Model
{
    public class RoutineReportModel
    {
        public string TenantId { get; set; }
        public int RequestSno { get; set; }
        public byte[] RequestBarCode { get; set; }
        public DateTime RequestDateTime { get; set; }
        public DateTime RequestedDateTime { get; set; }

        public string Name { get; set; }
        public string Gender { get; set; }
        public string DateofBirth { get; set; }
        public int AgeYears { get; set; }
        public int AgeMonths { get; set; }
        public int AgeDays { get; set; }
        public string Address { get; set; }
        public string MobileNo { get; set; }

        public Double RequestAmount { get; set; }

        public string Description { get; set; }
        public string CustCode { get; set; }
        public string Sample { get; set; }
        public string TestName { get; set; }
        public string GroupName { get; set; }

        public string Doctor { get; set; }
        public string Initial { get; set; }
        public string NameTitle { get; set; }
        public string Reference { get; set; }
        public string DoctorCode { get; set; }

        public string EnteredResult { get; set; }
        public string Reporting { get; set; }

        public string CityName { get; set; }
        public string AreaPinCode { get; set; }

        public string RequestGUID { get; set; }

        public int TestSno { get; set; }

        public string Col2 { get; set; }
        public string Units { get; set; }

        public int ResultSno { get; set; }

        public byte[]? CustomerImage { get; set; }
        public byte[]? SignatureImage { get; set; }

        public string ResultGUID { get; set; }

        public string ValueType { get; set; }
        public int TCode { get; set; }

        public DateTime? ResultDateTime { get; set; }

        public string ResultType { get; set; }

        public bool PrintInSeparatePage { get; set; }

        public int TestOrderNo { get; set; }
        public int GroupOrderNo { get; set; }

        public string RoomNo { get; set; }
        public string HospitalID { get; set; }

        public string Email { get; set; }
        public string AreaName { get; set; }
        public string AlteredBHCode { get; set; }

        public DateTime? CollectedDateTime { get; set; }

        public string OnlineCode { get; set; }
        public string ResultValueType { get; set; }

        public string DefaultValue { get; set; }
        public bool SimpleNormalValues { get; set; }
        public bool DetailedNormalValues { get; set; }

        public string RangeType { get; set; }

        public Double FromNormalValue { get; set; }
        public Double ToNormalValue { get; set; }

        public string ConclusionForHigher { get; set; }
        public string ConclusionForLower { get; set; }

        public bool ShowAgedBased { get; set; }
        public bool ShowAlertOnHigherLower { get; set; }

        public string FooterMessage { get; set; }

        public string TRPUName { get; set; }

        public string FixedValues { get; set; }

        public int DecimalPlaces { get; set; }

        public string ReportingMethod { get; set; }

        public Guid TestResultID { get; set; }

        public string RequestSnoPrint { get; set; }

        public bool PrintResultOnly { get; set; }

        public bool ResultNormal { get; set; }
        public bool ResultHigh { get; set; }
        public bool ResultLow { get; set; }

        public bool IsInvestigationPartial { get; set; }

        public string ResultSample { get; set; }

        public string DoctorFullName { get; set; }
        public int DCode { get; set; }

        public string FrontHospitalID { get; set; }
        public string FrontHospitalPatientID { get; set; }

        public bool IsAuthorized1 { get; set; }

        public string DoctorTitle { get; set; }
        public string SecondDoctorName { get; set; }
        public int SecondDCode { get; set; }

        public byte[]? DefaultAuthorizeImage { get; set; }
        public string DefaultAuthorizeName { get; set; }
        public string DefaultAuthorizeDesignation { get; set; }

        public string NormalValues { get; set; }

        public int RowNum { get; set; }

    }

    public class RoutineReportRequest
    {
        public IList<RoutineReportModel> results { get; set; }  
        // Header & Footer branding
        public byte[]? HeaderImage { get; set; }
        public byte[]? FooterImage { get; set; }
        public bool IsLetterhead { get; set; } = false;
    }
}
