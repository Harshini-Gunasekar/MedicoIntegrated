using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Booking.Models;
using SharedComponents.Rcl.Models;

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
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                var response = await _http.GetFromJsonAsync<List<RolePermissionItem>>("api/RoleMaster/master-roles?productId=MEDICO_APP", cts.Token);
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

        public async Task<List<long>> GetUserRolesAsync(long usercode)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<List<long>>($"api/RoleMaster/get-user-roles?usercode={usercode}");
                return response ?? new List<long>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleMasterService] Error fetching user roles: {ex.Message}");
                return new List<long>();
            }
        }

        public async Task<List<RolePermissionItem>> GetUserEffectivePermissionsAsync(long usercode)
        {
            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                var response = await _http.GetFromJsonAsync<List<RolePermissionItem>>($"api/RoleMaster/get-user-effective-permissions?usercode={usercode}", cts.Token);
                return response ?? new List<RolePermissionItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleMasterService] Error fetching user effective permissions: {ex.Message}");
                return new List<RolePermissionItem>();
            }
        }

        public async Task<(bool Success, string Message)> SaveUserRolesAsync(long usercode, List<long> roleIds)
        {
            try
            {
                var payload = new { usercode, Role_ids = roleIds };
                var response = await _http.PostAsJsonAsync("api/RoleMaster/save-user-roles", payload);
                if (response.IsSuccessStatusCode)
                {
                    return (true, "User roles updated successfully.");
                }
                var err = await response.Content.ReadAsStringAsync();
                return (false, string.IsNullOrWhiteSpace(err) ? "Failed to update user roles." : err);
            }
            catch (Exception ex)
            {
                return (false, $"Error updating user roles: {ex.Message}");
            }
        }

        // ===================== PERMISSION STATE & EVALUATION =====================

        public List<RolePermissionItem> CurrentUserPermissions { get; private set; } = new();
        public bool IsPermissionsLoaded { get; private set; } = false;

        private readonly HashSet<string> _permittedMainModules = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _permittedSubModules = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _permittedActions = new(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<long> _permittedRoleIds = new();

        public event Action? OnPermissionsChanged;

        private static string NormalizeKey(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return s.Replace(" ", "").Replace("_", "").Replace("-", "").ToLowerInvariant();
        }

        public void SetUserPermissions(IEnumerable<RolePermissionItem>? permissions)
        {
            CurrentUserPermissions = permissions?.ToList() ?? new List<RolePermissionItem>();
            _permittedMainModules.Clear();
            _permittedSubModules.Clear();
            _permittedActions.Clear();
            _permittedRoleIds.Clear();

            foreach (var p in CurrentUserPermissions)
            {
                _permittedRoleIds.Add(p.Role_id);

                var main = (p.Main_module ?? "").Trim();
                var sub = (p.Sub_module ?? "").Trim();
                var act = (p.Module ?? "").Trim();

                var normMain = NormalizeKey(main);
                var normSub = NormalizeKey(sub);
                var normAct = NormalizeKey(act);

                if (!string.IsNullOrEmpty(main))
                {
                    _permittedMainModules.Add(main);
                    _permittedMainModules.Add(normMain);
                }

                if (!string.IsNullOrEmpty(sub))
                {
                    _permittedSubModules.Add(sub);
                    _permittedSubModules.Add(normSub);
                    if (!string.IsNullOrEmpty(main))
                    {
                        _permittedSubModules.Add($"{main}::{sub}");
                        _permittedSubModules.Add($"{normMain}::{normSub}");
                    }
                }

                if (!string.IsNullOrEmpty(act))
                {
                    _permittedActions.Add(act);
                    _permittedActions.Add(normAct);
                    if (!string.IsNullOrEmpty(sub))
                    {
                        _permittedActions.Add($"{sub}::{act}");
                        _permittedActions.Add($"{normSub}::{normAct}");
                    }
                    if (!string.IsNullOrEmpty(main) && !string.IsNullOrEmpty(sub))
                    {
                        _permittedActions.Add($"{main}::{sub}::{act}");
                        _permittedActions.Add($"{normMain}::{normSub}::{normAct}");
                    }
                }
            }

            IsPermissionsLoaded = true;
            OnPermissionsChanged?.Invoke();
        }

        public async Task<List<RolePermissionItem>> LoadUserPermissionsAsync(long usercode)
        {
            if (usercode <= 0)
            {
                SetUserPermissions(new List<RolePermissionItem>());
                return CurrentUserPermissions;
            }

            var permissions = await GetUserEffectivePermissionsAsync(usercode);
            SetUserPermissions(permissions);
            return permissions;
        }

        public bool HasMainModule(string mainModule)
        {
            if (string.IsNullOrWhiteSpace(mainModule)) return true;
            var trimmed = mainModule.Trim();
            return _permittedMainModules.Contains(trimmed) || _permittedMainModules.Contains(NormalizeKey(trimmed));
        }

        public bool HasRight(string mainModule, string subModule)
        {
            var main = (mainModule ?? "").Trim();
            var sub = (subModule ?? "").Trim();

            if (string.IsNullOrEmpty(main) && string.IsNullOrEmpty(sub)) return true;

            var normMain = NormalizeKey(main);
            var normSub = NormalizeKey(sub);

            if (!string.IsNullOrEmpty(main) && !string.IsNullOrEmpty(sub))
            {
                if (_permittedSubModules.Contains($"{main}::{sub}") || 
                    _permittedSubModules.Contains($"{normMain}::{normSub}") ||
                    _permittedSubModules.Contains(sub) ||
                    _permittedSubModules.Contains(normSub))
                {
                    return true;
                }
            }
            else if (!string.IsNullOrEmpty(sub))
            {
                if (_permittedSubModules.Contains(sub) || _permittedSubModules.Contains(normSub))
                {
                    return true;
                }
            }
            else
            {
                return HasMainModule(main);
            }

            // Synonyms / Aliases
            if (normSub == "availabletheaters" && (_permittedSubModules.Contains("availabletheatre") || _permittedSubModules.Contains("availabletheater"))) return true;
            if (normSub == "availabletheatre" && _permittedSubModules.Contains("availabletheaters")) return true;
            if ((normSub == "summarytemplates" || normSub == "dischargesummarytemplates") && _permittedSubModules.Contains("dischargetemplatemaster")) return true;
            if (normSub == "dischargetemplatemaster" && _permittedSubModules.Contains("summarytemplates")) return true;
            if (normSub == "otmaster" && _permittedSubModules.Contains("addot")) return true;
            if (normSub == "addot" && _permittedSubModules.Contains("otmaster")) return true;
            if (normSub == "doctorspecialtymaster" && _permittedSubModules.Contains("doctorspecialitymaster")) return true;
            if (normSub == "doctorspecialitymaster" && _permittedSubModules.Contains("doctorspecialtymaster")) return true;
            if (normSub == "itemtypemaster" && (_permittedSubModules.Contains("subcategorymaster") || _permittedSubModules.Contains("categorymaster"))) return true;
            if (normSub == "subcategorymaster" && _permittedSubModules.Contains("itemtypemaster")) return true;
            if (normSub == "purchasepayment" && _permittedSubModules.Contains("purchasepayments")) return true;
            if (normSub == "purchasepayments" && _permittedSubModules.Contains("purchasepayment")) return true;
            if (normSub == "transferregister" && _permittedSubModules.Contains("storetransfer")) return true;
            if (normSub == "storetransfer" && _permittedSubModules.Contains("transferregister")) return true;
            if (normSub == "tokendisplayscreen" && _permittedSubModules.Contains("tokendisplay")) return true;
            if (normSub == "tokendisplay" && _permittedSubModules.Contains("tokendisplayscreen")) return true;
            if (normSub == "billlist" && (_permittedSubModules.Contains("billslist") || _permittedSubModules.Contains("bills"))) return true;
            if (normSub == "billslist" && _permittedSubModules.Contains("billlist")) return true;
            if (normSub == "billsummary" && _permittedSubModules.Contains("billingsummary")) return true;
            if (normSub == "billingsummary" && _permittedSubModules.Contains("billsummary")) return true;
            if (normSub == "patientcheckin" && (_permittedSubModules.Contains("walkin") || _permittedSubModules.Contains("patientcheckin"))) return true;

            return false;
        }

        public bool HasAction(string mainModule, string subModule, string actionName)
        {
            var main = (mainModule ?? "").Trim();
            var sub = (subModule ?? "").Trim();
            var act = (actionName ?? "").Trim();

            if (string.IsNullOrEmpty(act)) return HasRight(mainModule, subModule);

            var normMain = NormalizeKey(main);
            var normSub = NormalizeKey(sub);
            var normAct = NormalizeKey(act);

            if (!string.IsNullOrEmpty(main) && !string.IsNullOrEmpty(sub))
            {
                if (_permittedActions.Contains($"{main}::{sub}::{act}") ||
                    _permittedActions.Contains($"{normMain}::{normSub}::{normAct}"))
                {
                    return true;
                }
            }

            if (!string.IsNullOrEmpty(sub))
            {
                if (_permittedActions.Contains($"{sub}::{act}") ||
                    _permittedActions.Contains($"{normSub}::{normAct}"))
                {
                    return true;
                }
            }

            return _permittedActions.Contains(act) || _permittedActions.Contains(normAct);
        }

        public bool HasRoleId(long roleId)
        {
            return _permittedRoleIds.Contains(roleId);
        }

        public void ClearPermissions()
        {
            CurrentUserPermissions.Clear();
            _permittedMainModules.Clear();
            _permittedSubModules.Clear();
            _permittedActions.Clear();
            _permittedRoleIds.Clear();
            IsPermissionsLoaded = false;
            OnPermissionsChanged?.Invoke();
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

        public static List<User_Rights> ConvertEffectivePermissionsToUserRights(
            IEnumerable<RolePermissionItem>? permissions, 
            int userCode = 0)
        {
            var result = new List<User_Rights>();
            if (permissions == null) return result;

            var permList = permissions.ToList();
            if (!permList.Any()) return result;

            int sno = 1;
            int rightId = 1;

            var subGroups = permList
                .GroupBy(p => string.IsNullOrWhiteSpace(p.Sub_module) ? "General" : p.Sub_module.Trim())
                .ToList();

            bool hasAnyAdd = false;
            bool hasAnyEdit = false;
            bool hasAnyDelete = false;

            foreach (var group in subGroups)
            {
                var subName = group.Key;
                var mainName = group.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Main_module))?.Main_module?.Trim() ?? "General";

                var actionNames = group
                    .Select(p => (p.Module ?? "").Trim().ToLowerInvariant())
                    .Where(a => !string.IsNullOrEmpty(a))
                    .ToList();

                bool toAdd = actionNames.Any(a => 
                    a.Contains("create") || a.Contains("add") || a.Contains("new") || a.Contains("insert") || a.Contains("generate"));
                
                bool toEdit = actionNames.Any(a => 
                    a.Contains("edit") || a.Contains("update") || a.Contains("modify") || a.Contains("change") || a.Contains("save"));

                bool toDelete = actionNames.Any(a => 
                    a.Contains("delete") || a.Contains("remove") || a.Contains("cancel") || a.Contains("drop"));

                bool toExport = actionNames.Any(a => 
                    a.Contains("download") || a.Contains("print") || a.Contains("export") || a.Contains("excel") || a.Contains("pdf"));

                bool toView = actionNames.Any(a => 
                    a.Contains("view") || a.Contains("list") || a.Contains("dashboard") || a.Contains("report") || a.Contains("search") || a.Contains("read") || a.Contains("status") || a.Contains("preview") || a.Contains("display") || a.Contains("console") || a.Contains("notes")) || toAdd || toEdit || toDelete || toExport;

                if (toAdd) hasAnyAdd = true;
                if (toEdit) hasAnyEdit = true;
                if (toDelete) hasAnyDelete = true;

                // 1. Add raw sub module name
                result.Add(new User_Rights
                {
                    UserModuleID = rightId,
                    Sno = sno++,
                    ModuleName = subName,
                    Department = mainName,
                    UserModuleRightsID = rightId++,
                    UserCode = userCode,
                    ToAdd = toAdd,
                    ToView = toView,
                    ToEdit = toEdit,
                    ToDelete = toDelete,
                    ToExport = toExport
                });

                // 2. Add underscored alias e.g. "Ward_Master" if subName is "Ward Master"
                var underscored = subName.Replace(" ", "_");
                if (!string.Equals(underscored, subName, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(new User_Rights
                    {
                        UserModuleID = rightId,
                        Sno = sno++,
                        ModuleName = underscored,
                        Department = mainName,
                        UserModuleRightsID = rightId++,
                        UserCode = userCode,
                        ToAdd = toAdd,
                        ToView = toView,
                        ToEdit = toEdit,
                        ToDelete = toDelete,
                        ToExport = toExport
                    });
                }

                // 3. Add spaceless alias e.g. "WardMaster"
                var spaceless = subName.Replace(" ", "");
                if (!string.Equals(spaceless, subName, StringComparison.OrdinalIgnoreCase) && !string.Equals(spaceless, underscored, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(new User_Rights
                    {
                        UserModuleID = rightId,
                        Sno = sno++,
                        ModuleName = spaceless,
                        Department = mainName,
                        UserModuleRightsID = rightId++,
                        UserCode = userCode,
                        ToAdd = toAdd,
                        ToView = toView,
                        ToEdit = toEdit,
                        ToDelete = toDelete,
                        ToExport = toExport
                    });
                }
            }

            // Fallback general entries for TenantState.HasActionRight("Configuration", "General", "Create") etc.
            result.Add(new User_Rights
            {
                UserModuleID = rightId,
                Sno = sno++,
                ModuleName = "General",
                Department = "Configuration",
                UserModuleRightsID = rightId++,
                UserCode = userCode,
                ToAdd = hasAnyAdd,
                ToView = true,
                ToEdit = hasAnyEdit,
                ToDelete = hasAnyDelete,
                ToExport = true
            });

            result.Add(new User_Rights
            {
                UserModuleID = rightId,
                Sno = sno++,
                ModuleName = "General",
                Department = "General",
                UserModuleRightsID = rightId++,
                UserCode = userCode,
                ToAdd = hasAnyAdd,
                ToView = true,
                ToEdit = hasAnyEdit,
                ToDelete = hasAnyDelete,
                ToExport = true
            });

            return result;
        }
    }
}
