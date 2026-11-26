using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace SharedComponents.Rcl.Models
{
    public class FeatureAccess
    {
        [JsonPropertyName("View")] [JsonProperty("View")] public bool View { get; set; } = false;
        [JsonPropertyName("Create")] [JsonProperty("Create")] public bool Create { get; set; } = false;
        [JsonPropertyName("Edit")] [JsonProperty("Edit")] public bool Edit { get; set; } = false;
        [JsonPropertyName("Delete")] [JsonProperty("Delete")] public bool Delete { get; set; } = false;
        [JsonPropertyName("Export")] [JsonProperty("Export")] public bool Export { get; set; } = false;
        [JsonPropertyName("Chart")] [JsonProperty("Chart")] public bool Chart { get; set; } = false;
    }

    public class UserRights
    {
        [JsonPropertyName("chat_level")] [JsonProperty("chat_level")] public int chat_level { get; set; } = 1; 
        [JsonPropertyName("IsAdmin")] [JsonProperty("IsAdmin")] public bool IsAdmin { get; set; } = false;
        [JsonPropertyName("Features")] [JsonProperty("Features")] public Dictionary<string, FeatureAccess> Features { get; set; } = new();

        public bool HasAccess(string featureKey, string action = "View")
        {
            if (IsAdmin) return true; // Power User bypass
            if (Features == null) return false;
            
            // Case-insensitive lookup with standardized fallback
            var key = Features.Keys.FirstOrDefault(k => string.Equals(k, featureKey, StringComparison.OrdinalIgnoreCase));
            
            // Fallback for legacy keys if standardized key not found
            if (key == null)
            {
                if (featureKey.Equals("staff", StringComparison.OrdinalIgnoreCase))
                    key = Features.Keys.FirstOrDefault(k => string.Equals(k, "details", StringComparison.OrdinalIgnoreCase) || string.Equals(k, "rider", StringComparison.OrdinalIgnoreCase));
                else if (featureKey.Equals("client", StringComparison.OrdinalIgnoreCase))
                    key = Features.Keys.FirstOrDefault(k => string.Equals(k, "referral", StringComparison.OrdinalIgnoreCase));
            }

            if (key == null) return false;
            
            var access = Features[key];
            return action.ToLower() switch
            {
                "view" => access.View,
                "create" => access.Create,
                "edit" => access.Edit,
                "delete" => access.Delete,
                "export" => access.Export,
                "chart" => access.Chart,
                _ => false
            };
        }
    }
}

