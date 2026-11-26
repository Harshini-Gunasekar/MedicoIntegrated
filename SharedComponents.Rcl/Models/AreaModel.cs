using System.Text.Json.Serialization;

namespace SharedComponents.Rcl.Models
{
    public class AreaModel
    {
        public int areacode { get; set; }


        public int orderno { get; set; }
        public string? shortname { get; set; }
        public string? areaname { get; set; }
        public int? citycode { get; set; }
        public string? areapincode { get; set; }
        public int? statecode { get; set; }
        public int? countrycode { get; set; }
        public string? description { get; set; }
        public bool deleted { get; set; }
        public int usercode { get; set; }
        public int computercode { get; set; }
        public DateTimeOffset entereddate { get; set; }
        public DateTimeOffset ibsdate { get; set; }
        public double? homecollectioncharge { get; set; }
        public string? tenant_code { get; set; }

    }
}

