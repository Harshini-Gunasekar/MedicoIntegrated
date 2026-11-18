using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LabCare.Models
{
    [Table("test_fee_master")]
    public class TestFeeModel
    {
        [Key]
        public long tfcode { get; set; }

        public long tcode { get; set; }

        public long ftcode { get; set; }

        public string? margintype { get; set; }

        public double? marginpercentage { get; set; }

        public double? marginamount { get; set; }

        public double amount { get; set; }

        public double? feeper { get; set; }

        public double? feeamount { get; set; }

        public bool deleted { get; set; } = false;

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime entereddate { get; set; }

        public DateTime ibsdate { get; set; }

        public double? runningcost { get; set; }

        public string? commissiontype { get; set; }

        public string? tenant_code { get; set; }
    }

    public class Test_Fee_Master
    {
        [Key]
        public long tfcode { get; set; }

        public long tcode { get; set; }

        [JsonPropertyName("test_name")]
        public string? TestNameSnake { get; set; }

        [JsonPropertyName("group_name")]
        public string? GroupNameSnake { get; set; }

        [JsonPropertyName("testName")]
        public string? TestNameCamel { get; set; }

        [JsonPropertyName("groupName")]
        public string? GroupNameCamel { get; set; }

        [JsonPropertyName("name")]
        public string? GenericName { get; set; }

        // Helper properties for UI binding
        [JsonIgnore]
        public string DisplayTestName => TestNameCamel ?? TestNameSnake ?? GenericName ?? "N/A";

        [JsonIgnore]
        public string DisplayGroupName => GroupNameCamel ?? GroupNameSnake ?? "N/A";

        public decimal SpecialAmount { get; set; }

        public decimal PatientAmount { get; set; }

        public int GCode { get; set; }

        public long ftcode { get; set; }


        public string? margintype { get; set; }

        public double? marginpercentage { get; set; }

        public double? marginamount { get; set; }

        public double amount { get; set; }

        public double? feeper { get; set; }

        public double? feeamount { get; set; }

        public bool deleted { get; set; } = false;

        public int usercode { get; set; } = 1;

        public int computercode { get; set; } = 1;

        public DateTime entereddate { get; set; }

        public DateTime ibsdate { get; set; }

        public double? runningcost { get; set; }

        public string? commissiontype { get; set; }

        public string? tenant_code { get; set; }
    }
}
