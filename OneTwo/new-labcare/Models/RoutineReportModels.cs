using System;
using System.Collections.Generic;

namespace LabCare.Models
{
    public class RoutineReportDto
    {
        public string RequestGuid { get; set; } = "";
        public string Name { get; set; } = "";
        public string Age { get; set; } = "";
        public string Sex { get; set; } = "";
        public string Doctor { get; set; } = "";
        public string Sample { get; set; } = "";
        public DateTime Date { get; set; }
        public DateTime ReportDate { get; set; }
        public string RequestNo { get; set; } = "";
        public string BarCode { get; set; } = "";
        public string CollectionTime { get; set; } = "";
        public string EnteredBy { get; set; } = "";
        public string AuthorizedBy { get; set; } = "";
        public string AuthorizedBy2 { get; set; } = "";
        public string Hospital { get; set; } = "";
        public string ClientID { get; set; } = "";
        public string DataSource { get; set; } = "";
        public string ReportName { get; set; } = "";
        public string RequestGUID { get; set; } = "";
        public string TCode { get; set; } = "";
        public bool IsAuthorized2Finished { get; set; }
        public string QueryString { get; set; } = "";
        public List<ReportParameterDto> Parameters { get; set; } = new();
        public List<RoutineReportTestDto> Tests { get; set; } = new();
    }

    public class RoutineReportTestDto
    {
        public string TestName { get; set; } = "";
        public string Result { get; set; } = "";
        public string Unit { get; set; } = "";
        public string NormalValue { get; set; } = "";
        public bool IsAbnormal { get; set; }
        public string ResultValueType { get; set; } = "";
    }

    public class PreNatalRequest
    {
        public Guid RequestGUID { get; set; }
        public string TestName { get; set; } = "";

        // Extended fields used by CultureResultEntry report viewer
        public string PatientName { get; set; } = "";
        public string RequestSnoPrint { get; set; } = "";
        public string SkyCode { get; set; } = "";
    }
}
