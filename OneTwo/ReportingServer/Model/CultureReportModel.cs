using System;
using System.Collections.Generic;

namespace ReportingServer.Model
{
    public class CultureTestItemModel
    {
        public string Department { get; set; }
        public string ReportType { get; set; } 
        public string TestName { get; set; }
        public string? Specimen { get; set; }
        public string ReportingMethod { get; set; }
        public string Interpretation { get; set; }
        
        public string Diagnosis { get; set; } = "";
        public IList<OrganismModel> Organisms { get; set; } = new List<OrganismModel>();
    }

    public class AntibioticSensitivityModel
    {
        public string? AntibioticName { get; set; }
        public string? Result { get; set; }
        public string? MIC { get; set; }
    }

    public class OrganismModel
    {
        public string? OrganismName { get; set; }
        public string? ColonyCount { get; set; }
        public string? Comments { get; set; }
        public IList<AntibioticSensitivityModel>? Antibiotics { get; set; } = new List<AntibioticSensitivityModel>();
    }

    public class CultureReportDto
    {
        public string TenantId { get; set; }
        
        // Branding Byte Data
        public byte[]? logo { get; set; }
        public byte[]? headerimage { get; set; }
        public byte[]? footerimage { get; set; }
        public bool isletterhead { get; set; } = false;

        // Patient & Report Header Info
        public string SID { get; set; }
        public string PatId { get; set; }
        public string PatientName { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public string? DoctorName { get; set; }
        public string RefBy { get; set; }
        
        // Dates
        public DateTime CollectedOn { get; set; }
        public DateTime ReceivedOn { get; set; }
        public DateTime ReportedOn { get; set; }
        
        // Barcode Value
        public string Barcode { get; set; }

        // Dynamic Flat Properties for Single-Test Backward Compatibility
        public string? Department { get; set; }
        public string? ReportType { get; set; }
        public string? TestName { get; set; }
        public string? Specimen { get; set; }
        public string? ReportingMethod { get; set; }
        public string? Interpretation { get; set; }
        // Structured multi-organism / dynamic antibiotic properties
        public string? Diagnosis { get; set; }
        public IList<OrganismModel>? Organisms { get; set; } = new List<OrganismModel>();

        // List of multiple tests
        public IList<CultureTestItemModel>? Tests { get; set; } = new List<CultureTestItemModel>();
        // Doctor / Signature Info
        public byte[]? SignatureImage { get; set; }
        public string? SignatureDoctorName { get; set; }
        public string? SignatureDoctorDesignation { get; set; }
    }
}
