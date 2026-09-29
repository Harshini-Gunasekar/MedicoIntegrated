using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Booking.Models;

namespace Booking.Services
{
    public class RoleMasterService
    {
        private readonly HttpClient _http;

        private static readonly Dictionary<string, string> ModuleIcons = new(StringComparer.OrdinalIgnoreCase)
        {
            { "General", "bi-speedometer2" },
            { "OP Portal", "bi-door-open-fill" },
            { "IP Portal", "bi-hospital-fill" },
            { "Operation Theatre", "bi-heart-pulse-fill" },
            { "Billing Portal", "bi-credit-card-2-front-fill" },
            { "Laboratory", "bi-eyedropper" },
            { "Pharmacy", "bi-capsule" },
            { "Inventory", "bi-box-seam-fill" },
            { "Doctor Portal", "bi-person-heart" },
            { "Vitals", "bi-heart-fill" },
            { "Admin Portal", "bi-shield-lock-fill" }
        };

        public RoleMasterService(HttpClient http)
        {
            _http = http;
        }

        public string GetModuleIcon(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return "bi-folder2";
            return ModuleIcons.TryGetValue(moduleName, out var icon) ? icon : "bi-folder2";
        }

        public async Task<List<RolePermissionItem>> GetMasterCatalogAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<RolePermissionItem>>("api/RoleMaster/master-roles?productId=MEDICO_APP");
                if (response != null && response.Any())
                {
                    return response;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleMasterService] Failed to fetch live master catalog, using defaults: {ex.Message}");
            }

            // Instant 0ms fallback
            return RoleCatalogDefaults.GetDefaultPermissions();
        }

        public async Task<List<RoleSummaryDto>> GetRolesAsync()
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<RoleSummaryDto>>("api/RoleMaster/get-roles");
                return response ?? new List<RoleSummaryDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleMasterService] Error fetching roles: {ex.Message}");
                return new List<RoleSummaryDto>();
            }
        }

        public async Task<RoleSummaryDto?> GetRoleAsync(Guid roleGuid)
        {
            try
            {
                return await _http.GetFromJsonAsync<RoleSummaryDto>($"api/RoleMaster/get-role?roleGuid={roleGuid}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleMasterService] Error fetching role {roleGuid}: {ex.Message}");
                return null;
            }
        }

        public async Task<(bool Success, Guid? RoleGuid, string Message)> SaveRoleAsync(RoleSaveRequest req)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/RoleMaster/save-role", req);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(content);
                    Guid? guid = null;
                    if (doc.RootElement.TryGetProperty("roleGuid", out var guidProp) && guidProp.TryGetGuid(out var parsed))
                    {
                        guid = parsed;
                    }
                    return (true, guid, "Role template saved successfully.");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, null, string.IsNullOrWhiteSpace(err) ? "Failed to save role template." : err);
            }
            catch (Exception ex)
            {
                return (false, null, $"Error saving role template: {ex.Message}");
            }
        }

        public async Task<(bool Success, string Message)> DeleteRoleAsync(Guid roleGuid)
        {
            try
            {
                var response = await _http.DeleteAsync($"api/RoleMaster/delete-role?roleGuid={roleGuid}");
                if (response.IsSuccessStatusCode)
                {
                    return (true, "Role template deleted successfully.");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Failed to delete role template." : err);
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting role template: {ex.Message}");
            }
        }

        public List<ModuleTreeNode> BuildHierarchy(List<RolePermissionItem> permissions, HashSet<long>? selectedRoleIds = null)
        {
            selectedRoleIds ??= new HashSet<long>();

            var moduleNodes = new List<ModuleTreeNode>();

            // Group by Main Module
            var moduleGroups = permissions
                .GroupBy(p => string.IsNullOrWhiteSpace(p.Main_module) ? "General" : p.Main_module.Trim())
                .OrderBy(g => g.Min(p => p.Role_id));

            foreach (var modGroup in moduleGroups)
            {
                var modNode = new ModuleTreeNode
                {
                    ModuleName = modGroup.Key,
                    Icon = GetModuleIcon(modGroup.Key),
                    IsExpanded = true,
                    IsVisible = true,
                    SubModules = new List<SubModuleTreeNode>()
                };

                // Group by Sub Module
                var subGroups = modGroup
                    .GroupBy(p => string.IsNullOrWhiteSpace(p.Sub_module) ? "General" : p.Sub_module.Trim())
                    .OrderBy(g => g.Min(p => p.Role_id));

                foreach (var subGroup in subGroups)
                {
                    var subNode = new SubModuleTreeNode
                    {
                        SubModuleName = subGroup.Key,
                        IsExpanded = true,
                        IsVisible = true,
                        Actions = subGroup.Select(p => new ActionPermissionNode
                        {
                            Role_id = p.Role_id,
                            ActionName = string.IsNullOrWhiteSpace(p.Module) ? $"Right #{p.Role_id}" : p.Module.Trim(),
                            IsSelected = selectedRoleIds.Contains(p.Role_id),
                            IsVisible = true
                        }).ToList()
                    };

                    modNode.SubModules.Add(subNode);
                }

                moduleNodes.Add(modNode);
            }

            return moduleNodes;
        }
    }
}
