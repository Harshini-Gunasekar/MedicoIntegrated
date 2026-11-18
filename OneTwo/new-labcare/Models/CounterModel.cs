using System;
using Dapper.Contrib.Extensions;

namespace LabCare.Models
{
    [Table("counter_master")]
    public class CounterModel
    {
        [Key]
        public long? cntcode { get; set; }
        public int orderno { get; set; }
        public string? shortname { get; set; }
        public string name { get; set; }
        public string? description { get; set; }
        public int bhcode { get; set; }
        public string? cashlcode { get; set; }
        public int? patientbilllcode { get; set; }
        public int? referabillcode { get; set; }
        public int? patientsaleslcode { get; set; }
        public int? referalsaleslcode { get; set; }
        public int? commissionbilllcode { get; set; }
        public int? commissionsaleslcode { get; set; }
        public bool? timingcurrent { get; set; }
        public bool? timingvariable { get; set; }
        public DateTime? timingfrom { get; set; }
        public DateTime? timingto { get; set; }
        public bool? deleted { get; set; } = false;
        public int? usercode { get; set; }
        public int? computercode { get; set; }
        public DateTime? entereddate { get; set; }
        public DateTime? ibsdate { get; set; }
        public string? expenselcode { get; set; }
        public bool? timingfixed { get; set; }
        public decimal? bcolor { get; set; }
        public string? tenant_code { get; set; }
    }

    [Table("counter_timing")]
    public class CounterTimingModel
    {
        [ExplicitKey]
        public Guid? cnttid { get; set; }
        public int? cntcode { get; set; }
        public int? bhcode { get; set; }
        public int shiftsno { get; set; }
        public DateTime? counterdate { get; set; }
        public DateTime? fromdate { get; set; }
        public DateTime? todate { get; set; }
        public string? tenant_code { get; set; }
    }
}
