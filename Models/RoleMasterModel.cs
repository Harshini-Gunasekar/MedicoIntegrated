using System;
using System.Collections.Generic;

namespace Booking.Models
{
    public class RolePermissionItem
    {
        [System.Text.Json.Serialization.JsonPropertyName("role_id")]
        [Newtonsoft.Json.JsonProperty("role_id")]
        public long Role_id { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("main_module")]
        [Newtonsoft.Json.JsonProperty("main_module")]
        public string? Main_module { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("sub_module")]
        [Newtonsoft.Json.JsonProperty("sub_module")]
        public string? Sub_module { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("module")]
        [Newtonsoft.Json.JsonProperty("module")]
        public string? Module { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("product_id")]
        [Newtonsoft.Json.JsonProperty("product_id")]
        public string? Product_id { get; set; }
    }

    public class RoleSummaryDto
    {
        public Guid RoleNameGUID { get; set; }
        public string Role_Name { get; set; } = string.Empty;
        public string Tenant_code { get; set; } = string.Empty;
        public int TotalPermissionsAssigned { get; set; }
        public List<long> AssignedRoleIds { get; set; } = new();
        public List<string> MainModulesCovered { get; set; } = new();
    }

    public class RoleSaveRequest
    {
        public Guid? RoleNameGUID { get; set; }
        public string Role_Name { get; set; } = string.Empty;
        public List<long> Role_ids { get; set; } = new();
    }

    public class ActionPermissionNode
    {
        public long Role_id { get; set; }
        public string ActionName { get; set; } = string.Empty;
        public bool IsSelected { get; set; }
        public bool IsVisible { get; set; } = true;
    }

    public class SubModuleTreeNode
    {
        public string SubModuleName { get; set; } = string.Empty;
        public List<ActionPermissionNode> Actions { get; set; } = new();
        public bool IsExpanded { get; set; } = true;
        public bool IsVisible { get; set; } = true;

        public int SelectedCount => Actions.Count(a => a.IsSelected);
        public int TotalCount => Actions.Count;
        public bool IsAllSelected => TotalCount > 0 && SelectedCount == TotalCount;
    }

    public class ModuleTreeNode
    {
        public string ModuleName { get; set; } = string.Empty;
        public string Icon { get; set; } = "bi-folder";
        public List<SubModuleTreeNode> SubModules { get; set; } = new();
        public bool IsExpanded { get; set; } = true;
        public bool IsVisible { get; set; } = true;

        public int SelectedCount => SubModules.Sum(s => s.SelectedCount);
        public int TotalCount => SubModules.Sum(s => s.TotalCount);
        public bool IsAllSelected => TotalCount > 0 && SelectedCount == TotalCount;
    }
}
