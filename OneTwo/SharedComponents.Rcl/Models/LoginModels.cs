using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SharedComponents.Rcl.Models
{
    public class LC_LoginModel
    {
        public string? tenant_code { get; set; }
        public string? email { get; set; }
        public string? mobile { get; set; }
        public string identifier { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }

    public class LC_LoginDto
    {
        public string input { get; set; } = string.Empty;
        public string password { get; set; } = string.Empty;
    }

    public class LC_LoginResponseModel
    {
        public bool status { get; set; }
        public string? message { get; set; }
        public string? token { get; set; }
        public string? tenant_code { get; set; }
        public string? tenant_name { get; set; }
        public int? user_code { get; set; }
        public string? user_name { get; set; }
        public int? role_id { get; set; }
        public int? bhcode { get; set; }
        public int? cntcode { get; set; }
        public string? type { get; set; }
        public string? code { get; set; }
        public LC_LoginData? data { get; set; }
    }

    public class LC_LoginData
    {
        public string? token { get; set; }
        public string? tenant_code { get; set; }
        public string? tenant_name { get; set; }
        public int? user_code { get; set; }
        public string? user_name { get; set; }
        public string? short_name { get; set; }
        public int? role_id { get; set; }
        public int? bhcode { get; set; }
        public int? cntcode { get; set; }
        public List<string>? granted_modules { get; set; }
    }

    public class LC_UserSafeDto
    {
        [JsonPropertyName("usercode")]
        [JsonProperty("usercode")]
        public int usercode { get; set; }

        [JsonPropertyName("name")]
        [JsonProperty("name")]
        public string? name { get; set; }

        [JsonPropertyName("shortname")]
        [JsonProperty("shortname")]
        public string? shortname { get; set; }

        [JsonPropertyName("description")]
        [JsonProperty("description")]
        public string? description { get; set; }

        [JsonPropertyName("email")]
        [JsonProperty("email")]
        public string? email { get; set; }

        [JsonPropertyName("mobile")]
        [JsonProperty("mobile")]
        public string? mobile { get; set; }

        [JsonPropertyName("userimage")]
        [JsonProperty("userimage")]
        public string? userimage { get; set; }

        [JsonPropertyName("signatureimage")]
        [JsonProperty("signatureimage")]
        public string? signatureimage { get; set; }

        [JsonPropertyName("poweruser")]
        [JsonProperty("poweruser")]
        public bool poweruser { get; set; }

        [JsonPropertyName("bhcode")]
        [JsonProperty("bhcode")]
        public int? bhcode { get; set; }

        [JsonPropertyName("cntcode")]
        [JsonProperty("cntcode")]
        public int? cntcode { get; set; }

        [JsonPropertyName("tenant_code")]
        [JsonProperty("tenant_code")]
        public string? tenant_code { get; set; }

        [JsonPropertyName("address")]
        [JsonProperty("address")]
        public string? address { get; set; }

        [JsonPropertyName("role")]
        [JsonProperty("role")]
        public int? role { get; set; }

        [JsonPropertyName("module")]
        [JsonProperty("module")]
        public string? module { get; set; }
    }

    public class LC_LoginResponse
    {
        [JsonPropertyName("token")]
        [JsonProperty("token")]
        public string token { get; set; } = "";

        [JsonPropertyName("userdetails")]
        [JsonProperty("userdetails")]
        public LC_UserSafeDto userdetails { get; set; } = new();

        [JsonPropertyName("rights")]
        [JsonProperty("rights")]
        public IList<User_Rights> rights { get; set; } = new List<User_Rights>();
    }
}
