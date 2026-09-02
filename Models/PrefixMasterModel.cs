using Dapper.Contrib.Extensions;

namespace Medico_Backend.Model
{
    [Table("prefix_master")]
    public class PrefixMasterModel
    {
        [ExplicitKey]
        public int? prefixcode { get; set; }
        public string? prefixname { get; set; }
        public string? name { get; set; }
        public string? shortname { get; set; }
        public string DisplayName => !string.IsNullOrWhiteSpace(name) ? name : (!string.IsNullOrWhiteSpace(prefixname) ? prefixname : (!string.IsNullOrWhiteSpace(shortname) ? shortname : ""));
        public string? tenant_code { get; set; }
        public int? orderno { get; set; }
        public bool? deleted { get; set; } = false;
        public int? usercode { get; set; } = 1;
        public int? computercode { get; set; } = 1;
        public DateTime? entereddate { get; set; }
        public DateTime? ibsdate { get; set; }
    }
}
