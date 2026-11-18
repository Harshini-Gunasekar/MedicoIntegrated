using System.Text.Json.Serialization;

namespace LabCare.Models
{
    public class DepartmentModel
    {
        [JsonPropertyName("deptcode")]
        public int deptcode { get; set; }

        [JsonPropertyName("name")]
        public string name { get; set; } = string.Empty;

        [JsonPropertyName("shortname")]
        public string? shortname { get; set; }

        [JsonPropertyName("deleted")]
        public bool deleted { get; set; } = false;

        [JsonPropertyName("tenant_code")]
        public string? tenant_code { get; set; }
    }

    /// <summary>
    /// Represents a department row in the BillNo dept-wise mapping panel,
    /// tracking whether the row is selected and whether Scan / Lab flags apply.
    /// </summary>
    public class BillNoDeptRow
    {
        public int deptcode { get; set; }
        public string name { get; set; } = string.Empty;
        public bool selected { get; set; } = false;
        public bool isscan { get; set; } = false;
        public bool islab { get; set; } = false;
    }
}
