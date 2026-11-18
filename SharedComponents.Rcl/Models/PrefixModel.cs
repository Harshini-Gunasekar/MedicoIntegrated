using System;

namespace SharedComponents.Rcl.Models
{
    public class PrefixModel
    {
        public int pcode { get; set; }
        public string? name { get; set; }
        public string? prefix { get; set; }
        public string? tenant_code { get; set; }

        public string? DisplayName => !string.IsNullOrEmpty(name) ? name : prefix;
    }
}

