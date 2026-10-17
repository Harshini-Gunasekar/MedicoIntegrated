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
        private readonly IHttpClientFactory _clientFactory;

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

        private readonly SharedComponents.Rcl.Services.TenantSessionState? _tenantState;

        public RoleMasterService(IHttpClientFactory clientFactory, SharedComponents.Rcl.Services.TenantSessionState? tenantState = null)
        {
            _clientFactory = clientFactory;
            _tenantState = tenantState;
        }

        private HttpClient Client => _clientFactory.CreateClient("MedicoAPI");

        public string GetModuleIcon(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return "bi-folder2";
            return ModuleIcons.TryGetValue(moduleName, out var icon) ? icon : "bi-folder2";
        }

        public async Task<List<RolePermissionItem>> GetMasterCatalogAsync(string? tenantCode = null)
        {
            try
            {
                var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode)
                    ? tenantCode.Trim()
                    : (!string.IsNullOrWhiteSpace(_tenantState?.TenantCode) ? _tenantState.TenantCode.Trim() : null);

                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                using var req = new HttpRequestMessage(HttpMethod.Get, "api/RoleMaster/master-roles?productId=MEDICO_APP");
                if (!string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    req.Headers.TryAddWithoutValidation("tenant_code", effectiveTenant);
                    req.Headers.TryAddWithoutValidation("tenantcode", effectiveTenant);
                }

                var response = await Client.SendAsync(req, cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadFromJsonAsync<List<RolePermissionItem>>(cancellationToken: cts.Token);
                    if (data != null && data.Any())
                    {
                        var defaults = RoleCatalogDefaults.GetDefaultPermissions();
                        var existingIds = new HashSet<long>(data.Select(d => d.Role_id));
                        foreach (var def in defaults)
                        {
                            if (!existingIds.Contains(def.Role_id))
                            {
                                data.Add(def);
                            }
                        }
                        return data;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleMasterService] Failed to fetch live master catalog, using defaults: {ex.Message}");
            }

            // Instant 0ms fallback
            return RoleCatalogDefaults.GetDefaultPermissions();
        }

        public async Task<List<RoleSummaryDto>> GetRolesAsync(string? tenantCode = null)
        {
            try
            {
                var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode)
                    ? tenantCode.Trim()
                    : (!string.IsNullOrWhiteSpace(_tenantState?.TenantCode) ? _tenantState.TenantCode.Trim() : null);

                using var req = new HttpRequestMessage(HttpMethod.Get, "api/RoleMaster/get-roles");
                if (!string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    req.Headers.TryAddWithoutValidation("tenant_code", effectiveTenant);
                    req.Headers.TryAddWithoutValidation("tenantcode", effectiveTenant);
                }

                var res = await Client.SendAsync(req);
                if (res.IsSuccessStatusCode)
                {
                    var response = await res.Content.ReadFromJsonAsync<List<RoleSummaryDto>>();
                    if (response != null && !string.IsNullOrWhiteSpace(effectiveTenant))
                    {
                        return response.Where(r => string.Equals(r.Tenant_code, effectiveTenant, StringComparison.OrdinalIgnoreCase)).ToList();
                    }
                    return response ?? new List<RoleSummaryDto>();
                }
                else
                {
                    Console.WriteLine($"[RoleMasterService] Error fetching roles, status: {res.StatusCode}");
                    return new List<RoleSummaryDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleMasterService] Error fetching roles: {ex.Message}");
                return new List<RoleSummaryDto>();
            }
        }

        public async Task<RoleSummaryDto?> GetRoleAsync(Guid roleGuid, string? tenantCode = null)
        {
            try
            {
                var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode)
                    ? tenantCode.Trim()
                    : (!string.IsNullOrWhiteSpace(_tenantState?.TenantCode) ? _tenantState.TenantCode.Trim() : null);

                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/RoleMaster/get-role?roleGuid={roleGuid}");
                if (!string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    req.Headers.TryAddWithoutValidation("tenant_code", effectiveTenant);
                    req.Headers.TryAddWithoutValidation("tenantcode", effectiveTenant);
                }

                var res = await Client.SendAsync(req);
                if (res.IsSuccessStatusCode)
                {
                    return await res.Content.ReadFromJsonAsync<RoleSummaryDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleMasterService] Error fetching role {roleGuid}: {ex.Message}");
                return null;
            }
        }

        public async Task<(bool Success, Guid? RoleGuid, string Message)> SaveRoleAsync(RoleSaveRequest req, string? tenantCode = null)
        {
            try
            {
                var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode)
                    ? tenantCode.Trim()
                    : (!string.IsNullOrWhiteSpace(req.Tenant_code)
                        ? req.Tenant_code.Trim()
                        : (!string.IsNullOrWhiteSpace(_tenantState?.TenantCode) ? _tenantState.TenantCode.Trim() : null));

                if (!string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    req.Tenant_code = effectiveTenant;
                }

                using var reqMsg = new HttpRequestMessage(HttpMethod.Post, "api/RoleMaster/save-role");
                reqMsg.Content = JsonContent.Create(req);
                if (!string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    reqMsg.Headers.TryAddWithoutValidation("tenant_code", effectiveTenant);
                    reqMsg.Headers.TryAddWithoutValidation("tenantcode", effectiveTenant);
                }
                if (!string.IsNullOrWhiteSpace(_tenantState?.AuthToken))
                {
                    reqMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tenantState.AuthToken);
                }

                var response = await Client.SendAsync(reqMsg);
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

        public async Task<(bool Success, string Message)> DeleteRoleAsync(Guid roleGuid, string? tenantCode = null)
        {
            try
            {
                var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode)
                    ? tenantCode.Trim()
                    : (!string.IsNullOrWhiteSpace(_tenantState?.TenantCode) ? _tenantState.TenantCode.Trim() : null);

                using var reqMsg = new HttpRequestMessage(HttpMethod.Delete, $"api/RoleMaster/delete-role?roleGuid={roleGuid}");
                if (!string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    reqMsg.Headers.TryAddWithoutValidation("tenant_code", effectiveTenant);
                    reqMsg.Headers.TryAddWithoutValidation("tenantcode", effectiveTenant);
                }

                var response = await Client.SendAsync(reqMsg);
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

        public async Task<List<long>> GetUserRolesAsync(long usercode, string? tenantCode = null)
        {
            try
            {
                var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode)
                    ? tenantCode.Trim()
                    : (!string.IsNullOrWhiteSpace(_tenantState?.TenantCode) ? _tenantState.TenantCode.Trim() : null);

                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/RoleMaster/get-user-roles?usercode={usercode}");
                if (!string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    req.Headers.TryAddWithoutValidation("tenant_code", effectiveTenant);
                    req.Headers.TryAddWithoutValidation("tenantcode", effectiveTenant);
                }
                if (!string.IsNullOrWhiteSpace(_tenantState?.AuthToken))
                {
                    req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tenantState.AuthToken);
                }

                var res = await Client.SendAsync(req);
                if (res.IsSuccessStatusCode)
                {
                    return await res.Content.ReadFromJsonAsync<List<long>>() ?? new List<long>();
                }
                Console.WriteLine($"[RoleMasterService] Error fetching user roles status: {res.StatusCode}");
                return new List<long>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleMasterService] Error fetching user roles: {ex.Message}");
                return new List<long>();
            }
        }

        public async Task<List<RolePermissionItem>> GetUserEffectivePermissionsAsync(long usercode, string? tenantCode = null)
        {
            if (usercode <= 0) return new List<RolePermissionItem>();

            var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode)
                ? tenantCode.Trim()
                : (!string.IsNullOrWhiteSpace(_tenantState?.TenantCode) ? _tenantState.TenantCode.Trim() : null);

            var result = new List<RolePermissionItem>();

            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(10));
                using var req = new HttpRequestMessage(HttpMethod.Get, $"api/RoleMaster/get-user-effective-permissions?usercode={usercode}");
                if (!string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    req.Headers.TryAddWithoutValidation("tenant_code", effectiveTenant);
                    req.Headers.TryAddWithoutValidation("tenantcode", effectiveTenant);
                }
                if (!string.IsNullOrWhiteSpace(_tenantState?.AuthToken))
                {
                    req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tenantState.AuthToken);
                }

                var res = await Client.SendAsync(req, cts.Token);
                if (res.IsSuccessStatusCode)
                {
                    var content = await res.Content.ReadAsStringAsync(cts.Token);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RolePermissionItem>>(content);
                        if (list != null && list.Any())
                        {
                            result.AddRange(list);
                            Console.WriteLine($"[RoleMasterService] Successfully retrieved {list.Count} effective permissions for usercode {usercode} (tenant: {effectiveTenant})");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"[RoleMasterService] get-user-effective-permissions returned status code {res.StatusCode} for usercode {usercode} (tenant: {effectiveTenant})");
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleMasterService] Error fetching user effective permissions: {ex.Message}");
            }

            // CRITICAL: Always query user role IDs from DB and merge any missing catalog permissions (e.g. 1035-1046)
            // This ensures in-memory / newly registered catalog permissions (Purchase Order, AutoBill OP, Billing Categorization)
            // assigned to the user are never dropped even if backend get-user-effective-permissions only returns DB-persisted master roles.
            try
            {
                var roleIds = await GetUserRolesAsync(usercode, effectiveTenant);
                if (roleIds != null && roleIds.Any())
                {
                    var existingRoleIds = new HashSet<long>(result.Select(r => r.Role_id));
                    var missingRoleIds = roleIds.Where(id => !existingRoleIds.Contains(id)).ToList();
                    if (missingRoleIds.Any() || !result.Any())
                    {
                        var catalog = await GetMasterCatalogAsync(effectiveTenant);
                        var targetIds = missingRoleIds.Any() ? missingRoleIds : roleIds;
                        var additional = catalog.Where(c => targetIds.Contains(c.Role_id) && !existingRoleIds.Contains(c.Role_id)).ToList();
                        if (additional.Any())
                        {
                            result.AddRange(additional);
                            Console.WriteLine($"[RoleMasterService] Merged {additional.Count} missing permissions from user role IDs (e.g. 1035-1046) for usercode {usercode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleMasterService] Fallback get user roles error: {ex.Message}");
            }

            return result;
        }

        public async Task<(bool Success, string Message)> SaveUserRolesAsync(long usercode, List<long> roleIds, string? tenantCode = null)
        {
            try
            {
                var effectiveTenant = !string.IsNullOrWhiteSpace(tenantCode)
                    ? tenantCode.Trim()
                    : (!string.IsNullOrWhiteSpace(_tenantState?.TenantCode) ? _tenantState.TenantCode.Trim() : null);

                var payload = new { usercode, Role_ids = roleIds };
                using var reqMsg = new HttpRequestMessage(HttpMethod.Post, "api/RoleMaster/save-user-roles");
                reqMsg.Content = JsonContent.Create(payload);
                if (!string.IsNullOrWhiteSpace(effectiveTenant))
                {
                    reqMsg.Headers.TryAddWithoutValidation("tenant_code", effectiveTenant);
                    reqMsg.Headers.TryAddWithoutValidation("tenantcode", effectiveTenant);
                }
                if (!string.IsNullOrWhiteSpace(_tenantState?.AuthToken))
                {
                    reqMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tenantState.AuthToken);
                }

                var response = await Client.SendAsync(reqMsg);
                if (response.IsSuccessStatusCode)
                {
                    return (true, "User roles updated successfully.");
                }
                var err = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[RoleMasterService] save-user-roles failed ({response.StatusCode}): {err}");
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

        public static bool ArePermissionsEqual(IEnumerable<RolePermissionItem>? listA, IEnumerable<RolePermissionItem>? listB)
        {
            if (listA == null && listB == null) return true;
            if (listA == null || listB == null) return false;

            var itemsA = listA.ToList();
            var itemsB = listB.ToList();

            if (itemsA.Count != itemsB.Count) return false;

            var setA = new HashSet<long>(itemsA.Select(x => x.Role_id));
            var setB = new HashSet<long>(itemsB.Select(x => x.Role_id));

            if (!setA.SetEquals(setB)) return false;

            var pathSetA = new HashSet<string>(itemsA.Select(x => $"{x.Main_module?.Trim()}|{x.Sub_module?.Trim()}|{x.Module?.Trim()}"), StringComparer.OrdinalIgnoreCase);
            var pathSetB = new HashSet<string>(itemsB.Select(x => $"{x.Main_module?.Trim()}|{x.Sub_module?.Trim()}|{x.Module?.Trim()}"), StringComparer.OrdinalIgnoreCase);

            return pathSetA.SetEquals(pathSetB);
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

        public async Task<List<RolePermissionItem>> LoadUserPermissionsAsync(long usercode, string? tenantCode = null)
        {
            if (usercode <= 0)
            {
                SetUserPermissions(new List<RolePermissionItem>());
                return CurrentUserPermissions;
            }

            var permissions = await GetUserEffectivePermissionsAsync(usercode, tenantCode);
            SetUserPermissions(permissions);
            return permissions;
        }

        public bool HasMainModule(string mainModule)
        {
            if (string.IsNullOrWhiteSpace(mainModule)) return true;
            var trimmed = mainModule.Trim();
            var norm = NormalizeKey(trimmed);

            if (_permittedMainModules.Contains(trimmed) || _permittedMainModules.Contains(norm))
                return true;

            // Check department aliases
            if (norm == "opportal" && (_permittedMainModules.Contains("op") || _permittedMainModules.Contains("reception") || _permittedMainModules.Contains("outpatient") || _permittedMainModules.Contains("opd") || _permittedMainModules.Contains("opportal"))) return true;
            if (norm == "ipportal" && (_permittedMainModules.Contains("ip") || _permittedMainModules.Contains("ipd") || _permittedMainModules.Contains("inpatient") || _permittedMainModules.Contains("bed") || _permittedMainModules.Contains("ward") || _permittedMainModules.Contains("ipportal"))) return true;
            if ((norm == "operationtheatre" || norm == "operationtheater") && (_permittedMainModules.Contains("ot") || _permittedMainModules.Contains("operationtheatre") || _permittedMainModules.Contains("operationtheater") || _permittedMainModules.Contains("surgery"))) return true;
            if (norm == "billingportal" && (_permittedMainModules.Contains("billing") || _permittedMainModules.Contains("billingportal") || _permittedMainModules.Contains("cash") || _permittedMainModules.Contains("accounts"))) return true;
            if (norm == "laboratory" && (_permittedMainModules.Contains("lab") || _permittedMainModules.Contains("laboratory") || _permittedMainModules.Contains("diagnostics"))) return true;
            if (norm == "pharmacy" && (_permittedMainModules.Contains("pharmacy") || _permittedMainModules.Contains("pharma"))) return true;
            if (norm == "inventory" && (_permittedMainModules.Contains("inventory") || _permittedMainModules.Contains("store") || _permittedMainModules.Contains("stock") || _permittedMainModules.Contains("purchase"))) return true;
            if (norm == "doctorportal" && (_permittedMainModules.Contains("doctor") || _permittedMainModules.Contains("doctors") || _permittedMainModules.Contains("doctorportal") || _permittedMainModules.Contains("clinical"))) return true;
            if (norm == "vitals" && (_permittedMainModules.Contains("vital") || _permittedMainModules.Contains("vitals") || _permittedMainModules.Contains("vitalpage"))) return true;
            if (norm == "adminportal" && (_permittedMainModules.Contains("admin") || _permittedMainModules.Contains("administration") || _permittedMainModules.Contains("adminportal"))) return true;

            return false;
        }

        public bool HasRight(string subModule)
        {
            if (string.IsNullOrWhiteSpace(subModule)) return false;
            var normSub = NormalizeKey(subModule);
            if (_permittedSubModules.Contains(subModule.Trim()) || _permittedSubModules.Contains(normSub)) return true;
            return HasRight("IP Portal", subModule) || HasRight("OP Portal", subModule);
        }

        public bool HasRight(string mainModule, string subModule)
        {
            var main = (mainModule ?? "").Trim();
            var sub = (subModule ?? "").Trim();

            if (string.IsNullOrEmpty(main) && string.IsNullOrEmpty(sub)) return true;

            // If a specific main module/department is requested and user does not have that main module, deny
            if (!string.IsNullOrEmpty(main) && !HasMainModule(main))
            {
                return false;
            }

            var normMain = NormalizeKey(main);
            var normSub = NormalizeKey(sub);

            if (!string.IsNullOrEmpty(main) && !string.IsNullOrEmpty(sub))
            {
                if (_permittedSubModules.Contains($"{main}::{sub}") || 
                    _permittedSubModules.Contains($"{normMain}::{normSub}") ||
                    _permittedSubModules.Contains(sub) ||
                    _permittedSubModules.Contains(normSub) ||
                    _permittedActions.Contains($"{main}::{sub}") ||
                    _permittedActions.Contains($"{normMain}::{normSub}"))
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

            // Synonyms / Aliases (only evaluated if main module is permitted)
            if (normSub == "opdashboard" || normSub == "dashboard")
            {
                if (_permittedActions.Contains("opdashboard") || _permittedActions.Contains("dashboard") || _permittedSubModules.Contains("opdashboard") || _permittedMainModules.Contains("opportal") || _permittedMainModules.Contains("op") || _permittedMainModules.Contains("general"))
                    return true;
            }
            if (normSub == "ipdashboard")
            {
                if (_permittedActions.Contains("ipdashboard") || _permittedSubModules.Contains("ipdashboard") || _permittedMainModules.Contains("ipportal") || _permittedMainModules.Contains("ip"))
                    return true;
            }
            if (normSub == "patientcheckin" && (_permittedSubModules.Contains("walkin") || _permittedSubModules.Contains("patientcheckin") || _permittedSubModules.Contains("patientcheckin") || _permittedSubModules.Contains("reception") || _permittedSubModules.Contains("refferalcasecollection") || _permittedActions.Contains("createwalkin") || _permittedActions.Contains("viewwalkin"))) return true;
            if (normSub == "outpatientvisiting" && (_permittedSubModules.Contains("outpatientvisiting") || _permittedSubModules.Contains("outpatient") || _permittedSubModules.Contains("opdconsultation") || _permittedSubModules.Contains("casesheet") || _permittedSubModules.Contains("viewcasesheet") || _permittedActions.Contains("viewwalkin") || _permittedActions.Contains("createwalkin"))) return true;
            if (normSub == "patientmaster" && (_permittedSubModules.Contains("patientmaster") || _permittedSubModules.Contains("patientregistrationop") || _permittedSubModules.Contains("patientregistration"))) return true;
            if (normSub == "countrymaster" && (_permittedSubModules.Contains("countrymaster") || _permittedSubModules.Contains("country"))) return true;
            if (normSub == "statemaster" && (_permittedSubModules.Contains("statemaster") || _permittedSubModules.Contains("state"))) return true;
            if (normSub == "citymaster" && (_permittedSubModules.Contains("citymaster") || _permittedSubModules.Contains("city"))) return true;
            if (normSub == "areamaster" && (_permittedSubModules.Contains("areamaster") || _permittedSubModules.Contains("area"))) return true;
            if (normSub == "servicetypemaster" && (_permittedSubModules.Contains("servicetypemaster") || _permittedSubModules.Contains("servicetype"))) return true;
            if ((normSub == "slotmaster" || normSub == "slots" || normSub == "slot") && (_permittedSubModules.Contains("slotmaster") || _permittedSubModules.Contains("slots") || _permittedSubModules.Contains("slot") || _permittedRoleIds.Contains(383) || _permittedRoleIds.Contains(661))) return true;
            if (normSub == "optokendisplay" && (_permittedActions.Contains("viewtokendisplay") || _permittedSubModules.Contains("optokendisplay") || _permittedSubModules.Contains("tokendisplay") || _permittedSubModules.Contains("tokendisplayscreen") || _permittedMainModules.Contains("opportal") || _permittedMainModules.Contains("op"))) return true;
            if (normSub == "tokendisplayscreen" && (_permittedSubModules.Contains("tokendisplay") || _permittedSubModules.Contains("tokendisplayscreen") || _permittedSubModules.Contains("optokendisplay") || _permittedActions.Contains("tokendisplay") || _permittedActions.Contains("viewtokendisplay"))) return true;
            if (normSub == "tokendisplay" && (_permittedSubModules.Contains("tokendisplayscreen") || _permittedSubModules.Contains("tokendisplay") || _permittedSubModules.Contains("optokendisplay"))) return true;

            // IP Portal
            if (normSub == "ipregistration" && (_permittedSubModules.Contains("ipregistration") || _permittedSubModules.Contains("admissionip"))) return true;
            if (normSub == "admissionip" && (_permittedSubModules.Contains("ipregistration") || _permittedSubModules.Contains("admissionip"))) return true;
            if (normSub == "availablebeds" && (_permittedSubModules.Contains("availablebeds") || _permittedActions.Contains("availablebeds"))) return true;
            if (normSub == "bedtransfer" && (_permittedSubModules.Contains("bedtransfer") || _permittedActions.Contains("bedtransfer"))) return true;
            if ((normSub == "dischargesummary" || normSub == "dischargesummery") && (_permittedSubModules.Contains("dischargesummary") || _permittedSubModules.Contains("dischargesummery"))) return true;
            if (normSub == "nursenotes" && (_permittedSubModules.Contains("nursenotes") || _permittedActions.Contains("nursenotes"))) return true;
            if (normSub == "bedmaster" && (_permittedSubModules.Contains("bedmaster") || _permittedSubModules.Contains("beds"))) return true;
            if (normSub == "wardmaster" && (_permittedSubModules.Contains("wardmaster") || _permittedSubModules.Contains("wards"))) return true;
            if (normSub == "roomtypemaster" && (_permittedSubModules.Contains("roomtypemaster") || _permittedSubModules.Contains("roomtypes"))) return true;
            if (normSub == "floormaster" && (_permittedSubModules.Contains("floormaster") || _permittedSubModules.Contains("floors"))) return true;
            if (normSub == "blockmaster" && (_permittedSubModules.Contains("blockmaster") || _permittedSubModules.Contains("blocks"))) return true;
            if (normSub == "nursemaster" && (_permittedSubModules.Contains("nursemaster") || _permittedSubModules.Contains("nurses"))) return true;
            if (normSub == "ioparticularsmaster" && (_permittedSubModules.Contains("ioparticularsmaster") || _permittedSubModules.Contains("ioparticulars"))) return true;
            if (normSub == "servicenamemaster" && (_permittedSubModules.Contains("servicenamemaster") || _permittedSubModules.Contains("servicename"))) return true;
            if (normSub == "scheduletypemaster" && (_permittedSubModules.Contains("scheduletypemaster") || _permittedSubModules.Contains("scheduletype"))) return true;
            if (normSub == "availabletheaters" && (_permittedSubModules.Contains("availabletheatre") || _permittedSubModules.Contains("availabletheater") || _permittedActions.Contains("availabletheaters"))) return true;
            if (normSub == "availabletheatre" && _permittedSubModules.Contains("availabletheaters")) return true;
            if ((normSub == "summarytemplates" || normSub == "dischargesummarytemplates" || normSub == "summerytemplates") && (_permittedSubModules.Contains("dischargetemplatemaster") || _permittedSubModules.Contains("summarytemplates") || _permittedSubModules.Contains("summerytemplates"))) return true;
            if (normSub == "dischargetemplatemaster" && (_permittedSubModules.Contains("summarytemplates") || _permittedSubModules.Contains("summerytemplates"))) return true;
            if (normSub == "otmaster" && (_permittedSubModules.Contains("addot") || _permittedActions.Contains("addot") || _permittedSubModules.Contains("otmaster"))) return true;
            if (normSub == "addot" && _permittedSubModules.Contains("otmaster")) return true;
            if (normSub == "doctorspecialtymaster" && (_permittedSubModules.Contains("doctorspecialitymaster") || _permittedSubModules.Contains("doctorspecialties") || _permittedSubModules.Contains("doctorspecialty"))) return true;
            if (normSub == "doctorspecialitymaster" && (_permittedSubModules.Contains("doctorspecialtymaster") || _permittedSubModules.Contains("doctorspecialties"))) return true;
            if (normSub == "itemtypemaster" && (_permittedSubModules.Contains("subcategorymaster") || _permittedSubModules.Contains("categorymaster") || _permittedSubModules.Contains("itemtypemaster"))) return true;
            if (normSub == "subcategorymaster" && _permittedSubModules.Contains("itemtypemaster")) return true;
            if ((normSub == "purchase" || normSub == "purchasemaster") &&
                (_permittedSubModules.Contains("purchase") || _permittedSubModules.Contains("purchasemaster") || _permittedRoleIds.Contains(590) || _permittedActions.Contains("purchase"))) return true;
            if ((normSub == "purchaseorder" || normSub == "purchaseorders" || normSub == "po") &&
                (_permittedSubModules.Contains("purchaseorder") || _permittedSubModules.Contains("purchaseorders") || _permittedRoleIds.Contains(1035) || _permittedActions.Contains("purchaseorder"))) return true;
            if (normSub == "purchasereturn" && (_permittedSubModules.Contains("purchasereturn") || _permittedActions.Contains("purchasereturn"))) return true;
            if (normSub == "purchasepayment" && (_permittedSubModules.Contains("purchasepayments") || _permittedSubModules.Contains("purchasepayment") || _permittedSubModules.Contains("purchasefilter"))) return true;
            if (normSub == "purchasepayments" && (_permittedSubModules.Contains("purchasepayment") || _permittedSubModules.Contains("purchasefilter"))) return true;
            if (normSub == "purchasefilter" && (_permittedSubModules.Contains("purchasepayments") || _permittedSubModules.Contains("purchasepayment") || _permittedSubModules.Contains("purchasefilter"))) return true;
            if (normSub == "indentrequest" && (_permittedSubModules.Contains("indentrequest") || _permittedActions.Contains("indentrequest"))) return true;
            if (normSub == "indentapproval" && (_permittedSubModules.Contains("indentapproval") || _permittedActions.Contains("indentapproval"))) return true;
            if ((normSub == "stock" || normSub == "openingstock") && (_permittedSubModules.Contains("stock") || _permittedSubModules.Contains("openingstock") || _permittedActions.Contains("stock"))) return true;
            if (normSub == "stockadjustment" && (_permittedSubModules.Contains("stockadjustment") || _permittedActions.Contains("stockadjustment"))) return true;
            if (normSub == "issuestock" && (_permittedSubModules.Contains("issuestock") || _permittedActions.Contains("issuestock"))) return true;
            if (normSub == "receivestock" && (_permittedSubModules.Contains("receivestock") || _permittedActions.Contains("receivestock"))) return true;
            if (normSub == "transferregister" && (_permittedSubModules.Contains("storetransfer") || _permittedSubModules.Contains("transferregister"))) return true;
            if (normSub == "storetransfer" && (_permittedSubModules.Contains("transferregister") || _permittedSubModules.Contains("storetransfer"))) return true;
            if (normSub == "doctorconsumption" && (_permittedSubModules.Contains("doctorconsumption") || _permittedActions.Contains("doctorconsumption"))) return true;
            if (normSub == "itemmaster" && (_permittedSubModules.Contains("itemmaster") || _permittedSubModules.Contains("items") || _permittedActions.Contains("itemmaster"))) return true;
            if ((normSub == "categorymaster" || normSub == "parentcategorymaster") && (_permittedSubModules.Contains("categorymaster") || _permittedSubModules.Contains("parentcategorymaster"))) return true;
            if (normSub == "vendormaster" && (_permittedSubModules.Contains("vendormaster") || _permittedSubModules.Contains("vendors") || _permittedActions.Contains("vendormaster"))) return true;
            if ((normSub == "unitmaster" || normSub == "uommaster") && (_permittedSubModules.Contains("unitmaster") || _permittedSubModules.Contains("uommaster") || _permittedSubModules.Contains("unit"))) return true;
            if ((normSub == "storemaster" || normSub == "warehousemaster") && (_permittedSubModules.Contains("storemaster") || _permittedSubModules.Contains("warehousemaster") || _permittedSubModules.Contains("warehouse") || _permittedSubModules.Contains("store"))) return true;
            if (normSub == "manufacturermaster" && (_permittedSubModules.Contains("manufacturermaster") || _permittedSubModules.Contains("manufacturers") || _permittedSubModules.Contains("manufacturer"))) return true;
            if (normSub == "ledgergroup" && (_permittedSubModules.Contains("ledgergroup") || _permittedSubModules.Contains("ledgergroups"))) return true;
            if (normSub == "ledgertype" && (_permittedSubModules.Contains("ledgertype") || _permittedSubModules.Contains("ledgermaster") || _permittedSubModules.Contains("ledgertypes"))) return true;
            if (normSub == "billlist" && (_permittedSubModules.Contains("billslist") || _permittedSubModules.Contains("bills") || _permittedSubModules.Contains("billlist"))) return true;
            if (normSub == "billslist" && (_permittedSubModules.Contains("billlist") || _permittedSubModules.Contains("bills"))) return true;
            if (normSub == "billsummary" && (_permittedSubModules.Contains("billingsummary") || _permittedSubModules.Contains("billsummary"))) return true;
            if (normSub == "billingsummary" && (_permittedSubModules.Contains("billsummary") || _permittedSubModules.Contains("billingsummary"))) return true;
            if ((normSub == "billingcategorization" || normSub == "billingcategories" || normSub == "opcategorycollection" || normSub == "categorycollection") &&
                (_permittedSubModules.Contains("billingcategorization") || _permittedSubModules.Contains("billingcategories") || _permittedSubModules.Contains("opcategorycollection") || _permittedRoleIds.Contains(1043) || _permittedRoleIds.Contains(1044) || _permittedRoleIds.Contains(1045) || _permittedRoleIds.Contains(1046))) return true;
            if ((normSub == "autobillop" || normSub == "autobilloprequest" || normSub == "autobill") &&
                (_permittedSubModules.Contains("autobillop") || _permittedSubModules.Contains("autobilloprequest") || _permittedSubModules.Contains("autobill") || _permittedRoleIds.Contains(1039) || _permittedRoleIds.Contains(1040) || _permittedRoleIds.Contains(1041) || _permittedRoleIds.Contains(1042))) return true;
            if ((normSub == "cashcounter" || normSub == "cashcounterportal") && (_permittedSubModules.Contains("cashcounter") || _permittedSubModules.Contains("cashcounterportal"))) return true;
            if ((normSub == "countershift" || normSub == "countershifts") && (_permittedSubModules.Contains("countershift") || _permittedSubModules.Contains("countershifts"))) return true;
            if (normSub == "reimbursementco" && (_permittedSubModules.Contains("reimbursementcompanymaster") || _permittedSubModules.Contains("reimbursementcompany") || _permittedSubModules.Contains("reimbursements") || _permittedSubModules.Contains("reimbursementco"))) return true;
            if (normSub == "reimbursementcompany" && (_permittedSubModules.Contains("reimbursementco") || _permittedSubModules.Contains("reimbursementcompanymaster"))) return true;
            if (normSub == "settings" && (_permittedSubModules.Contains("settings") || _permittedSubModules.Contains("labsettings") || _permittedSubModules.Contains("reportsettings"))) return true;
            if (normSub == "labsettings" && (_permittedSubModules.Contains("settings") || _permittedSubModules.Contains("labsettings"))) return true;
            if (normSub == "userrights" && (_permittedSubModules.Contains("userrights") || _permittedSubModules.Contains("userrightstemplate") || _permittedSubModules.Contains("usermasterrights"))) return true;
            if (normSub == "productfeature" || normSub == "productfeatures" || normSub == "productfeatureregistry")
            {
                if (_permittedSubModules.Contains("productfeatures") || _permittedSubModules.Contains("productfeatureregistry") || _permittedSubModules.Contains("productfeature")) return true;
            }

            if (normSub == "doctorpatientlist" && (_permittedSubModules.Contains("doctorpatientlist") || _permittedSubModules.Contains("patientlist") || _permittedSubModules.Contains("doctorappointments") || _permittedActions.Contains("doctorpatientlist"))) return true;
            if (normSub == "doctorstatus" && (_permittedSubModules.Contains("doctorstatus") || _permittedSubModules.Contains("doctorcurrentstatus") || _permittedActions.Contains("doctorstatus"))) return true;
            if (normSub == "doctorcurrentstatus" && (_permittedSubModules.Contains("doctorstatus") || _permittedSubModules.Contains("doctorcurrentstatus"))) return true;
            if ((normSub == "doctorsmaster" || normSub == "doctormaster" || normSub == "doctors") && (_permittedSubModules.Contains("doctorsmaster") || _permittedSubModules.Contains("doctormaster") || _permittedSubModules.Contains("doctors"))) return true;
            if ((normSub == "slottypemaster" || normSub == "slottypes") && (_permittedSubModules.Contains("slottypemaster") || _permittedSubModules.Contains("slottypes"))) return true;
            if ((normSub == "doctortypemaster" || normSub == "doctortypes") && (_permittedSubModules.Contains("doctortypemaster") || _permittedSubModules.Contains("doctortypes"))) return true;
            if ((normSub == "doctorspecialitymaster" || normSub == "doctorspecialtymaster" || normSub == "doctorspecialties" || normSub == "doctorspecialty") && (_permittedSubModules.Contains("doctorspecialitymaster") || _permittedSubModules.Contains("doctorspecialtymaster") || _permittedSubModules.Contains("doctorspecialties"))) return true;

            // Vitals Portal Submodules
            if ((normSub == "vitalsdashboard" || normSub == "vitaldashboard") && (_permittedSubModules.Contains("vitalsdashboard") || _permittedSubModules.Contains("vitaldashboard") || _permittedSubModules.Contains("dashboard") || _permittedRoleIds.Contains(685))) return true;
            if ((normSub == "vitalsentry" || normSub == "vitals" || normSub == "vitalentry") && (_permittedSubModules.Contains("vitalsentry") || _permittedSubModules.Contains("vitals") || _permittedSubModules.Contains("vitalentry") || _permittedRoleIds.Contains(686))) return true;
            if ((normSub == "labtestentry" || normSub == "labtest") && (_permittedSubModules.Contains("labtestentry") || _permittedSubModules.Contains("labtest") || _permittedRoleIds.Contains(690))) return true;
            if ((normSub == "scanresultentry" || normSub == "scanresult" || normSub == "scantestentry") && (_permittedSubModules.Contains("scanresultentry") || _permittedSubModules.Contains("scanresult") || _permittedRoleIds.Contains(693))) return true;
            if ((normSub == "tokendisplayscreen" || normSub == "tokendisplay") && (_permittedSubModules.Contains("tokendisplayscreen") || _permittedSubModules.Contains("tokendisplay") || _permittedSubModules.Contains("optokendisplay") || _permittedRoleIds.Contains(696) || _permittedRoleIds.Contains(382))) return true;
            if ((normSub == "appointmentprebooking" || normSub == "appointprebooking" || normSub == "prebooking") && (_permittedSubModules.Contains("appointmentprebooking") || _permittedSubModules.Contains("appointprebooking") || _permittedRoleIds.Contains(697))) return true;
            if ((normSub == "receptionportal" || normSub == "receptionbooking") && (_permittedSubModules.Contains("receptionportal") || _permittedSubModules.Contains("receptionbooking") || _permittedRoleIds.Contains(701))) return true;
            if (normSub == "ogscreen" && (_permittedSubModules.Contains("ogscreen") || _permittedRoleIds.Contains(703))) return true;
            if ((normSub == "doctorgroupmaster" || normSub == "doctorgroups") && (_permittedSubModules.Contains("doctorgroupmaster") || _permittedSubModules.Contains("doctorgroups") || _permittedSubModules.Contains("groupmaster") || _permittedRoleIds.Contains(705))) return true;

            // Admin Portal Submodules
            if ((normSub == "usermaster" || normSub == "users") && (_permittedSubModules.Contains("usermaster") || _permittedSubModules.Contains("users") || _permittedRoleIds.Contains(709))) return true;
            if ((normSub == "userrights" || normSub == "userrightstemplate" || normSub == "usermasterrights") && (_permittedSubModules.Contains("userrights") || _permittedSubModules.Contains("userrightstemplate") || _permittedRoleIds.Contains(713))) return true;
            if ((normSub == "productfeatures" || normSub == "productfeature" || normSub == "productfeatureregistry") && (_permittedSubModules.Contains("productfeatures") || _permittedSubModules.Contains("productfeatureregistry") || _permittedRoleIds.Contains(715))) return true;
            if ((normSub == "branchmaster" || normSub == "branch" || normSub == "branches") && (_permittedSubModules.Contains("branchmaster") || _permittedSubModules.Contains("branch") || _permittedRoleIds.Contains(719))) return true;
            if ((normSub == "settings" || normSub == "labsettings") && (_permittedSubModules.Contains("settings") || _permittedSubModules.Contains("labsettings") || _permittedRoleIds.Contains(723))) return true;

            // Laboratory Portal Submodules
            if ((normSub == "resultsearch" || normSub == "labresultsearch") && (_permittedSubModules.Contains("resultsearch") || _permittedSubModules.Contains("labresultsearch") || _permittedRoleIds.Contains(535))) return true;
            if ((normSub == "resultentry" || normSub == "labresultentry" || normSub == "labresultdataentry") && (_permittedSubModules.Contains("resultentry") || _permittedSubModules.Contains("labresultentry") || _permittedRoleIds.Contains(537))) return true;
            if ((normSub == "testmaster" || normSub == "testmasters" || normSub == "tests") && (_permittedSubModules.Contains("testmaster") || _permittedSubModules.Contains("testmasters") || _permittedRoleIds.Contains(542))) return true;
            if ((normSub == "billitems" || normSub == "billitem") && (_permittedSubModules.Contains("billitems") || _permittedSubModules.Contains("billitem") || _permittedRoleIds.Contains(546))) return true;
            if ((normSub == "testfeemaster" || normSub == "testfee") && (_permittedSubModules.Contains("testfeemaster") || _permittedSubModules.Contains("testfee") || _permittedRoleIds.Contains(550))) return true;
            if ((normSub == "testtypemaster" || normSub == "testtypes") && (_permittedSubModules.Contains("testtypemaster") || _permittedSubModules.Contains("testtypes") || _permittedRoleIds.Contains(553))) return true;
            if ((normSub == "groupmaster" || normSub == "groups") && (_permittedSubModules.Contains("groupmaster") || _permittedSubModules.Contains("groups") || _permittedRoleIds.Contains(557))) return true;
            if ((normSub == "reportmethods" || normSub == "reportmethodmaster" || normSub == "methodmaster") && (_permittedSubModules.Contains("reportmethods") || _permittedSubModules.Contains("reportmethodmaster") || _permittedRoleIds.Contains(561))) return true;
            if ((normSub == "specimenmaster" || normSub == "samplemaster" || normSub == "samples") && (_permittedSubModules.Contains("specimenmaster") || _permittedSubModules.Contains("samplemaster") || _permittedRoleIds.Contains(565))) return true;
            if ((normSub == "uommaster" || normSub == "unitmaster" || normSub == "units") && (_permittedSubModules.Contains("uommaster") || _permittedSubModules.Contains("unitmaster") || _permittedRoleIds.Contains(569))) return true;
            if ((normSub == "machinemaster" || normSub == "machines") && (_permittedSubModules.Contains("machinemaster") || _permittedSubModules.Contains("machines") || _permittedRoleIds.Contains(573))) return true;

            return false;
        }

        public bool HasAction(string subModule, string actionName)
        {
            if (string.IsNullOrWhiteSpace(subModule) || string.IsNullOrWhiteSpace(actionName)) return false;
            var normSub = NormalizeKey(subModule);
            var normAct = NormalizeKey(actionName);
            if (_permittedActions.Contains($"{subModule.Trim()}::{actionName.Trim()}") || _permittedActions.Contains($"{normSub}::{normAct}")) return true;
            return HasAction("IP Portal", subModule, actionName) || HasAction("OP Portal", subModule, actionName) || HasAction("Inventory", subModule, actionName) || HasAction("Billing Portal", subModule, actionName) || HasAction("Pharmacy", subModule, actionName) || HasAction("Doctor Portal", subModule, actionName) || HasAction("Vitals", subModule, actionName) || HasAction("Admin Portal", subModule, actionName) || HasAction("Laboratory", subModule, actionName);
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

                // Department aliases (e.g., "opportal" <-> "op", "ipportal" <-> "ip")
                if (normMain == "opportal" && (_permittedActions.Contains($"op::{normSub}::{normAct}") || _permittedActions.Contains($"op::{sub}::{act}"))) return true;
                if (normMain == "op" && (_permittedActions.Contains($"opportal::{normSub}::{normAct}") || _permittedActions.Contains($"opportal::{sub}::{act}"))) return true;
                if (normMain == "ipportal" && (_permittedActions.Contains($"ip::{normSub}::{normAct}") || _permittedActions.Contains($"ip::{sub}::{act}"))) return true;
                if (normMain == "ip" && (_permittedActions.Contains($"ipportal::{normSub}::{normAct}") || _permittedActions.Contains($"ipportal::{sub}::{act}"))) return true;
                if (normMain == "billingportal" && (_permittedActions.Contains($"billing::{normSub}::{normAct}") || _permittedActions.Contains($"billing::{sub}::{act}"))) return true;
                if (normMain == "billing" && (_permittedActions.Contains($"billingportal::{normSub}::{normAct}") || _permittedActions.Contains($"billingportal::{sub}::{act}"))) return true;
                if (normMain == "inventory" && (_permittedActions.Contains($"store::{normSub}::{normAct}") || _permittedActions.Contains($"store::{sub}::{act}"))) return true;
                if (normMain == "store" && (_permittedActions.Contains($"inventory::{normSub}::{normAct}") || _permittedActions.Contains($"inventory::{sub}::{act}"))) return true;
                if (normMain == "doctorportal" && (_permittedActions.Contains($"doctor::{normSub}::{normAct}") || _permittedActions.Contains($"doctor::{sub}::{act}"))) return true;
                if (normMain == "doctor" && (_permittedActions.Contains($"doctorportal::{normSub}::{normAct}") || _permittedActions.Contains($"doctorportal::{sub}::{act}"))) return true;
                if (normMain == "vitals" && (_permittedActions.Contains($"vital::{normSub}::{normAct}") || _permittedActions.Contains($"vitalpage::{normSub}::{normAct}"))) return true;
                if (normMain == "vital" && (_permittedActions.Contains($"vitals::{normSub}::{normAct}") || _permittedActions.Contains($"vitals::{sub}::{act}"))) return true;
                if (normMain == "adminportal" && (_permittedActions.Contains($"admin::{normSub}::{normAct}") || _permittedActions.Contains($"administration::{normSub}::{normAct}"))) return true;
                if (normMain == "admin" && (_permittedActions.Contains($"adminportal::{normSub}::{normAct}") || _permittedActions.Contains($"adminportal::{sub}::{act}"))) return true;
                if (normMain == "laboratory" && (_permittedActions.Contains($"lab::{normSub}::{normAct}") || _permittedActions.Contains($"lab::{sub}::{act}") || _permittedActions.Contains($"diagnostics::{normSub}::{normAct}"))) return true;
                if ((normMain == "lab" || normMain == "diagnostics") && (_permittedActions.Contains($"laboratory::{normSub}::{normAct}") || _permittedActions.Contains($"laboratory::{sub}::{act}"))) return true;

                // Submodule synonyms
                if (normSub == "patientmaster" && (_permittedActions.Contains($"{normMain}::patientregistrationop::{normAct}") || _permittedActions.Contains($"patientregistrationop::{normAct}"))) return true;
                if (normSub == "patientregistrationop" && (_permittedActions.Contains($"{normMain}::patientmaster::{normAct}") || _permittedActions.Contains($"patientmaster::{normAct}"))) return true;
                if ((normSub == "ipregistration" || normSub == "admissionip") && (normAct == "create" || normAct == "register" || normAct == "add" || normAct == "registerinpatient") && _permittedRoleIds.Contains(413)) return true;
                if ((normSub == "ipregistration" || normSub == "admissionip") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(414)) return true;
                if ((normSub == "ipregistration" || normSub == "admissionip") && (normAct == "delete" || normAct == "cancel" || normAct == "canceladmission") && _permittedRoleIds.Contains(415)) return true;
                if ((normSub == "ipregistration" || normSub == "admissionip") && (normAct == "preview" || normAct == "previewreport" || normAct == "previewbill" || normAct == "previewconsolidatedbill") && _permittedRoleIds.Contains(416)) return true;
                if ((normSub == "ipregistration" || normSub == "admissionip") && (normAct == "download" || normAct == "downloadreport" || normAct == "downloadbill" || normAct == "downloadconsolidatedbillpdf" || normAct == "pdf") && _permittedRoleIds.Contains(417)) return true;
                if ((normSub == "ipregistration" || normSub == "admissionip") && (normAct == "casesheet" || normAct == "viewcasesheet") && (_permittedRoleIds.Contains(418) || _permittedRoleIds.Contains(419))) return true;
                if ((normSub == "ipregistration" || normSub == "admissionip") && (normAct == "dischargesummary" || normAct == "viewdischargesummary") && (_permittedRoleIds.Contains(420) || _permittedRoleIds.Contains(421) || _permittedRoleIds.Contains(422) || _permittedRoleIds.Contains(428))) return true;

                if (CheckInventoryRoleAction(normSub, normAct)) return true;
                if (CheckDoctorRoleAction(normSub, normAct)) return true;
                if (CheckVitalsRoleAction(normSub, normAct)) return true;
                if (CheckAdminRoleAction(normSub, normAct)) return true;
                if (CheckBillingReportsRoleAction(normSub, normAct)) return true;
                if (CheckLaboratoryRoleAction(normSub, normAct)) return true;

                // Submodule-level action match (with sub, without main)
                if (_permittedActions.Contains($"{sub}::{act}") ||
                    _permittedActions.Contains($"{normSub}::{normAct}"))
                {
                    return true;
                }

                return false;
            }

            if (!string.IsNullOrEmpty(sub))
            {
                if (_permittedActions.Contains($"{sub}::{act}") ||
                    _permittedActions.Contains($"{normSub}::{normAct}"))
                {
                    return true;
                }

                if (normSub == "patientmaster" && _permittedActions.Contains($"patientregistrationop::{normAct}")) return true;
                if (normSub == "patientregistrationop" && _permittedActions.Contains($"patientmaster::{normAct}")) return true;
                if ((normSub == "ipregistration" || normSub == "admissionip") && (normAct == "create" || normAct == "register" || normAct == "add" || normAct == "registerinpatient") && _permittedRoleIds.Contains(413)) return true;
                if ((normSub == "ipregistration" || normSub == "admissionip") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(414)) return true;
                if ((normSub == "ipregistration" || normSub == "admissionip") && (normAct == "delete" || normAct == "cancel" || normAct == "canceladmission") && _permittedRoleIds.Contains(415)) return true;
                if ((normSub == "ipregistration" || normSub == "admissionip") && (normAct == "preview" || normAct == "previewreport" || normAct == "previewbill" || normAct == "previewconsolidatedbill") && _permittedRoleIds.Contains(416)) return true;
                if ((normSub == "ipregistration" || normSub == "admissionip") && (normAct == "download" || normAct == "downloadreport" || normAct == "downloadbill" || normAct == "downloadconsolidatedbillpdf" || normAct == "pdf") && _permittedRoleIds.Contains(417)) return true;
                if ((normSub == "ipregistration" || normSub == "admissionip") && (normAct == "casesheet" || normAct == "viewcasesheet") && (_permittedRoleIds.Contains(418) || _permittedRoleIds.Contains(419))) return true;
                if ((normSub == "ipregistration" || normSub == "admissionip") && (normAct == "dischargesummary" || normAct == "viewdischargesummary") && (_permittedRoleIds.Contains(420) || _permittedRoleIds.Contains(421) || _permittedRoleIds.Contains(422) || _permittedRoleIds.Contains(428))) return true;

                if (CheckInventoryRoleAction(normSub, normAct)) return true;
                if (CheckDoctorRoleAction(normSub, normAct)) return true;
                if (CheckVitalsRoleAction(normSub, normAct)) return true;
                if (CheckAdminRoleAction(normSub, normAct)) return true;
                if (CheckBillingReportsRoleAction(normSub, normAct)) return true;

                return false;
            }

            if (!string.IsNullOrEmpty(main))
            {
                if (_permittedActions.Contains($"{main}::{act}") ||
                    _permittedActions.Contains($"{normMain}::{normAct}"))
                {
                    return true;
                }
                return false;
            }

            return _permittedActions.Contains(act) || _permittedActions.Contains(normAct);
        }

        private bool CheckInventoryRoleAction(string normSub, string normAct)
        {
            // Dedicated Purchase Order (1035 - 1038)
            if ((normSub == "purchaseorder" || normSub == "purchaseorders" || normSub == "po") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(1035)) return true;
            if ((normSub == "purchaseorder" || normSub == "purchaseorders" || normSub == "po") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(1036)) return true;
            if ((normSub == "purchaseorder" || normSub == "purchaseorders" || normSub == "po") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(1037)) return true;
            if ((normSub == "purchaseorder" || normSub == "purchaseorders" || normSub == "po") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(1038)) return true;

            if ((normSub == "purchase" || normSub == "purchasemaster") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(590)) return true;
            if ((normSub == "purchase" || normSub == "purchasemaster") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(591)) return true;
            if ((normSub == "purchase" || normSub == "purchasemaster") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(592)) return true;
            if ((normSub == "purchase" || normSub == "purchasemaster") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(593)) return true;

            if (normSub == "purchasereturn" && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(594)) return true;
            if (normSub == "purchasereturn" && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(595)) return true;

            if ((normSub == "purchasepayments" || normSub == "purchasepayment" || normSub == "purchasefilter") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(596)) return true;
            if ((normSub == "purchasepayments" || normSub == "purchasepayment" || normSub == "purchasefilter") && (normAct == "create" || normAct == "add" || normAct == "pay" || normAct == "payment") && _permittedRoleIds.Contains(597)) return true;

            if (normSub == "indentrequest" && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(598)) return true;
            if (normSub == "indentrequest" && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(599)) return true;
            if (normSub == "indentrequest" && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(600)) return true;
            if (normSub == "indentrequest" && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(601)) return true;

            if (normSub == "indentapproval" && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(602)) return true;
            if (normSub == "indentapproval" && (normAct == "create" || normAct == "add" || normAct == "approve" || normAct == "approval") && _permittedRoleIds.Contains(603)) return true;

            if ((normSub == "stock" || normSub == "openingstock") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(604)) return true;
            if ((normSub == "stock" || normSub == "openingstock") && (normAct == "preview" || normAct == "previewreport") && _permittedRoleIds.Contains(605)) return true;
            if ((normSub == "stock" || normSub == "openingstock") && (normAct == "print" || normAct == "printreport") && _permittedRoleIds.Contains(606)) return true;

            if (normSub == "stockadjustment" && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(607)) return true;
            if (normSub == "stockadjustment" && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(608)) return true;

            if (normSub == "issuestock" && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(609)) return true;
            if (normSub == "issuestock" && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(610)) return true;
            if (normSub == "issuestock" && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(611)) return true;
            if (normSub == "issuestock" && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(612)) return true;

            if (normSub == "receivestock" && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(613)) return true;
            if (normSub == "receivestock" && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(614)) return true;

            if ((normSub == "transferregister" || normSub == "storetransfer") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(615)) return true;

            if (normSub == "doctorconsumption" && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(616)) return true;
            if (normSub == "doctorconsumption" && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(617)) return true;

            if ((normSub == "itemmaster" || normSub == "items") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(618)) return true;
            if ((normSub == "itemmaster" || normSub == "items") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(619)) return true;
            if ((normSub == "itemmaster" || normSub == "items") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(620)) return true;
            if ((normSub == "itemmaster" || normSub == "items") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(621)) return true;

            if ((normSub == "categorymaster" || normSub == "parentcategorymaster") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(622)) return true;
            if ((normSub == "categorymaster" || normSub == "parentcategorymaster") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(623)) return true;
            if ((normSub == "categorymaster" || normSub == "parentcategorymaster") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(624)) return true;
            if ((normSub == "categorymaster" || normSub == "parentcategorymaster") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(625)) return true;

            if ((normSub == "itemtypemaster" || normSub == "subcategorymaster") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(626)) return true;
            if ((normSub == "itemtypemaster" || normSub == "subcategorymaster") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(627)) return true;
            if ((normSub == "itemtypemaster" || normSub == "subcategorymaster") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(628)) return true;
            if ((normSub == "itemtypemaster" || normSub == "subcategorymaster") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(629)) return true;

            if ((normSub == "vendormaster" || normSub == "vendors") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(630)) return true;
            if ((normSub == "vendormaster" || normSub == "vendors") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(631)) return true;
            if ((normSub == "vendormaster" || normSub == "vendors") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(632)) return true;
            if ((normSub == "vendormaster" || normSub == "vendors") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(633)) return true;

            if ((normSub == "unitmaster" || normSub == "uommaster") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(634)) return true;
            if ((normSub == "unitmaster" || normSub == "uommaster") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(635)) return true;
            if ((normSub == "unitmaster" || normSub == "uommaster") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(636)) return true;
            if ((normSub == "unitmaster" || normSub == "uommaster") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(637)) return true;

            if ((normSub == "storemaster" || normSub == "warehousemaster") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(638)) return true;
            if ((normSub == "storemaster" || normSub == "warehousemaster") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(639)) return true;
            if ((normSub == "storemaster" || normSub == "warehousemaster") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(640)) return true;
            if ((normSub == "storemaster" || normSub == "warehousemaster") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(641)) return true;

            if ((normSub == "manufacturermaster" || normSub == "manufacturers") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(642)) return true;
            if ((normSub == "manufacturermaster" || normSub == "manufacturers") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(643)) return true;
            if ((normSub == "manufacturermaster" || normSub == "manufacturers") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(644)) return true;
            if ((normSub == "manufacturermaster" || normSub == "manufacturers") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(645)) return true;

            if ((normSub == "ledgergroup" || normSub == "ledgergroups") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(646)) return true;
            if ((normSub == "ledgergroup" || normSub == "ledgergroups") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(647)) return true;
            if ((normSub == "ledgergroup" || normSub == "ledgergroups") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(648)) return true;
            if ((normSub == "ledgergroup" || normSub == "ledgergroups") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(649)) return true;

            if ((normSub == "ledgertype" || normSub == "ledgertypes") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(650)) return true;
            if ((normSub == "ledgertype" || normSub == "ledgertypes") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(651)) return true;
            if ((normSub == "ledgertype" || normSub == "ledgertypes") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(652)) return true;
            if ((normSub == "ledgertype" || normSub == "ledgertypes") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(653)) return true;

            // Purchase Order (1035 - 1038)
            if ((normSub == "purchaseorder" || normSub == "purchaseorders" || normSub == "po") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(1035)) return true;
            if ((normSub == "purchaseorder" || normSub == "purchaseorders" || normSub == "po") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(1036)) return true;
            if ((normSub == "purchaseorder" || normSub == "purchaseorders" || normSub == "po") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(1037)) return true;
            if ((normSub == "purchaseorder" || normSub == "purchaseorders" || normSub == "po") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(1038)) return true;

            return false;
        }

        private bool CheckDoctorRoleAction(string normSub, string normAct)
        {
            // 1. Doctor Patient List (654 - 657)
            if ((normSub == "doctorpatientlist" || normSub == "patientlist" || normSub == "doctorappointments") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(654)) return true;
            if ((normSub == "doctorpatientlist" || normSub == "patientlist" || normSub == "doctorappointments") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(655)) return true;
            if ((normSub == "doctorpatientlist" || normSub == "patientlist" || normSub == "doctorappointments") && (normAct == "finalize" || normAct == "finalizecasesheet" || normAct == "casesheet") && _permittedRoleIds.Contains(656)) return true;
            if ((normSub == "doctorpatientlist" || normSub == "patientlist" || normSub == "doctorappointments") && (normAct == "print" || normAct == "printcasesheet") && _permittedRoleIds.Contains(657)) return true;

            // 2. Doctor Status (658 - 660)
            if ((normSub == "doctorstatus" || normSub == "doctorcurrentstatus") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(658)) return true;
            if ((normSub == "doctorstatus" || normSub == "doctorcurrentstatus") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(659)) return true;
            if ((normSub == "doctorstatus" || normSub == "doctorcurrentstatus") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(660)) return true;

            // 3. Slot Master (661 - 664, plus OP 383 - 386)
            if ((normSub == "slotmaster" || normSub == "slots" || normSub == "slot") && (normAct == "view" || normAct == "list" || normAct == "viewslot") && (_permittedRoleIds.Contains(661) || _permittedRoleIds.Contains(383))) return true;
            if ((normSub == "slotmaster" || normSub == "slots" || normSub == "slot") && (normAct == "create" || normAct == "add" || normAct == "createslot") && (_permittedRoleIds.Contains(662) || _permittedRoleIds.Contains(384))) return true;
            if ((normSub == "slotmaster" || normSub == "slots" || normSub == "slot") && (normAct == "edit" || normAct == "update" || normAct == "editslot") && (_permittedRoleIds.Contains(663) || _permittedRoleIds.Contains(385))) return true;
            if ((normSub == "slotmaster" || normSub == "slots" || normSub == "slot") && (normAct == "delete" || normAct == "cancel" || normAct == "deleteslot") && (_permittedRoleIds.Contains(664) || _permittedRoleIds.Contains(386))) return true;

            // 4. Doctors Master (665 - 668)
            if ((normSub == "doctorsmaster" || normSub == "doctormaster" || normSub == "doctors") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(665)) return true;
            if ((normSub == "doctorsmaster" || normSub == "doctormaster" || normSub == "doctors") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(666)) return true;
            if ((normSub == "doctorsmaster" || normSub == "doctormaster" || normSub == "doctors") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(667)) return true;
            if ((normSub == "doctorsmaster" || normSub == "doctormaster" || normSub == "doctors") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(668)) return true;

            // 5. Slot Type Master (669 - 672)
            if ((normSub == "slottypemaster" || normSub == "slottypes") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(669)) return true;
            if ((normSub == "slottypemaster" || normSub == "slottypes") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(670)) return true;
            if ((normSub == "slottypemaster" || normSub == "slottypes") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(671)) return true;
            if ((normSub == "slottypemaster" || normSub == "slottypes") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(672)) return true;

            // 6. Doctor Type Master (673 - 676)
            if ((normSub == "doctortypemaster" || normSub == "doctortypes") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(673)) return true;
            if ((normSub == "doctortypemaster" || normSub == "doctortypes") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(674)) return true;
            if ((normSub == "doctortypemaster" || normSub == "doctortypes") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(675)) return true;
            if ((normSub == "doctortypemaster" || normSub == "doctortypes") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(676)) return true;

            // 7. Doctor Speciality Master (677 - 680)
            if ((normSub == "doctorspecialitymaster" || normSub == "doctorspecialtymaster" || normSub == "doctorspecialties" || normSub == "doctorspecialty") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(677)) return true;
            if ((normSub == "doctorspecialitymaster" || normSub == "doctorspecialtymaster" || normSub == "doctorspecialties" || normSub == "doctorspecialty") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(678)) return true;
            if ((normSub == "doctorspecialitymaster" || normSub == "doctorspecialtymaster" || normSub == "doctorspecialties" || normSub == "doctorspecialty") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(679)) return true;
            if ((normSub == "doctorspecialitymaster" || normSub == "doctorspecialtymaster" || normSub == "doctorspecialties" || normSub == "doctorspecialty") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(680)) return true;

            // 8. Reimbursement Co (681 - 684, plus Billing 531)
            if ((normSub == "reimbursementco" || normSub == "reimbursementcompany" || normSub == "reimbursementcompanymaster") && (normAct == "view" || normAct == "list") && (_permittedRoleIds.Contains(681) || _permittedRoleIds.Contains(531))) return true;
            if ((normSub == "reimbursementco" || normSub == "reimbursementcompany" || normSub == "reimbursementcompanymaster") && (normAct == "create" || normAct == "add") && (_permittedRoleIds.Contains(682) || _permittedRoleIds.Contains(532))) return true;
            if ((normSub == "reimbursementco" || normSub == "reimbursementcompany" || normSub == "reimbursementcompanymaster") && (normAct == "edit" || normAct == "update") && (_permittedRoleIds.Contains(683) || _permittedRoleIds.Contains(533))) return true;
            if ((normSub == "reimbursementco" || normSub == "reimbursementcompany" || normSub == "reimbursementcompanymaster") && (normAct == "delete" || normAct == "cancel") && (_permittedRoleIds.Contains(684) || _permittedRoleIds.Contains(534))) return true;

            return false;
        }

        private bool CheckVitalsRoleAction(string normSub, string normAct)
        {
            // 1. Vitals Dashboard (685)
            if ((normSub == "vitalsdashboard" || normSub == "dashboard" || normSub == "general") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(685)) return true;

            // 2. Vitals Entry (686 - 689)
            if ((normSub == "vitalsentry" || normSub == "vitals" || normSub == "vitalentry") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(686)) return true;
            if ((normSub == "vitalsentry" || normSub == "vitals" || normSub == "vitalentry") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(687)) return true;
            if ((normSub == "vitalsentry" || normSub == "vitals" || normSub == "vitalentry") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(688)) return true;
            if ((normSub == "vitalsentry" || normSub == "vitals" || normSub == "vitalentry") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(689)) return true;

            // 3. Lab Test Entry (690 - 692)
            if ((normSub == "labtestentry" || normSub == "labtest") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(690)) return true;
            if ((normSub == "labtestentry" || normSub == "labtest") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(691)) return true;
            if ((normSub == "labtestentry" || normSub == "labtest") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(692)) return true;

            // 4. Scan Result Entry (693 - 695)
            if ((normSub == "scanresultentry" || normSub == "scanresult" || normSub == "scantestentry") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(693)) return true;
            if ((normSub == "scanresultentry" || normSub == "scanresult" || normSub == "scantestentry") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(694)) return true;
            if ((normSub == "scanresultentry" || normSub == "scanresult" || normSub == "scantestentry") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(695)) return true;

            // 5. Token Display Screen (696, plus OP 382)
            if ((normSub == "tokendisplayscreen" || normSub == "tokendisplay" || normSub == "optokendisplay") && (normAct == "view" || normAct == "list") && (_permittedRoleIds.Contains(696) || _permittedRoleIds.Contains(382))) return true;

            // 6. Appointment Pre-Booking (697 - 700)
            if ((normSub == "appointmentprebooking" || normSub == "appointprebooking" || normSub == "prebooking") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(697)) return true;
            if ((normSub == "appointmentprebooking" || normSub == "appointprebooking" || normSub == "prebooking") && (normAct == "create" || normAct == "add" || normAct == "book") && _permittedRoleIds.Contains(698)) return true;
            if ((normSub == "appointmentprebooking" || normSub == "appointprebooking" || normSub == "prebooking") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(699)) return true;
            if ((normSub == "appointmentprebooking" || normSub == "appointprebooking" || normSub == "prebooking") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(700)) return true;

            // 7. Reception Portal (701 - 702)
            if ((normSub == "receptionportal" || normSub == "receptionbooking") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(701)) return true;
            if ((normSub == "receptionportal" || normSub == "receptionbooking") && (normAct == "create" || normAct == "add" || normAct == "checkin") && _permittedRoleIds.Contains(702)) return true;

            // 8. OG Screen (703 - 704)
            if (normSub == "ogscreen" && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(703)) return true;
            if (normSub == "ogscreen" && (normAct == "create" || normAct == "add" || normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(704)) return true;

            // 9. Doctor Group Master (705 - 708)
            if ((normSub == "doctorgroupmaster" || normSub == "doctorgroups" || normSub == "groupmaster") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(705)) return true;
            if ((normSub == "doctorgroupmaster" || normSub == "doctorgroups" || normSub == "groupmaster") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(706)) return true;
            if ((normSub == "doctorgroupmaster" || normSub == "doctorgroups" || normSub == "groupmaster") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(707)) return true;
            if ((normSub == "doctorgroupmaster" || normSub == "doctorgroups" || normSub == "groupmaster") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(708)) return true;

            return false;
        }

        private bool CheckAdminRoleAction(string normSub, string normAct)
        {
            // 1. User Master (709 - 712)
            if ((normSub == "usermaster" || normSub == "users") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(709)) return true;
            if ((normSub == "usermaster" || normSub == "users") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(710)) return true;
            if ((normSub == "usermaster" || normSub == "users") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(711)) return true;
            if ((normSub == "usermaster" || normSub == "users") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(712)) return true;

            // 2. User Rights (713 - 714)
            if ((normSub == "userrights" || normSub == "userrightstemplate" || normSub == "usermasterrights") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(713)) return true;
            if ((normSub == "userrights" || normSub == "userrightstemplate" || normSub == "usermasterrights") && (normAct == "create" || normAct == "add" || normAct == "edit" || normAct == "update" || normAct == "save" || normAct == "create&edit") && _permittedRoleIds.Contains(714)) return true;

            // 3. Product & Feature Registry (715 - 718)
            if ((normSub == "productfeatures" || normSub == "productfeature" || normSub == "productfeatureregistry") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(715)) return true;
            if ((normSub == "productfeatures" || normSub == "productfeature" || normSub == "productfeatureregistry") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(716)) return true;
            if ((normSub == "productfeatures" || normSub == "productfeature" || normSub == "productfeatureregistry") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(717)) return true;
            if ((normSub == "productfeatures" || normSub == "productfeature" || normSub == "productfeatureregistry") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(718)) return true;

            // 4. Branch Master (719 - 722)
            if ((normSub == "branchmaster" || normSub == "branch" || normSub == "branches") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(719)) return true;
            if ((normSub == "branchmaster" || normSub == "branch" || normSub == "branches") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(720)) return true;
            if ((normSub == "branchmaster" || normSub == "branch" || normSub == "branches") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(721)) return true;
            if ((normSub == "branchmaster" || normSub == "branch" || normSub == "branches") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(722)) return true;

            // 5. Settings (723 - 724)
            if ((normSub == "settings" || normSub == "labsettings" || normSub == "reportsettings") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(723)) return true;
            if ((normSub == "settings" || normSub == "labsettings" || normSub == "reportsettings") && (normAct == "create" || normAct == "add" || normAct == "edit" || normAct == "update" || normAct == "save") && _permittedRoleIds.Contains(724)) return true;

            return false;
        }

        private bool CheckBillingReportsRoleAction(string normSub, string normAct)
        {
            if ((normSub == "billingportal" || normSub == "billing") && (normAct == "editqty") && _permittedRoleIds.Contains(725)) return true;
            if ((normSub == "billingportal" || normSub == "billing") && (normAct == "editconsession" || normAct == "editdiscount") && _permittedRoleIds.Contains(726)) return true;
            if ((normSub == "reports" || normSub == "billingreports") && (normAct == "preview" || normAct == "previewreport") && _permittedRoleIds.Contains(727)) return true;
            if ((normSub == "reports" || normSub == "billingreports") && (normAct == "print" || normAct == "printreport") && _permittedRoleIds.Contains(728)) return true;

            // Auto Bill OP (1039 - 1042)
            if ((normSub == "autobillop" || normSub == "autobilloprequest" || normSub == "autobill") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(1039)) return true;
            if ((normSub == "autobillop" || normSub == "autobilloprequest" || normSub == "autobill") && (normAct == "create" || normAct == "add" || normAct == "move" || normAct == "movetocash") && _permittedRoleIds.Contains(1040)) return true;
            if ((normSub == "autobillop" || normSub == "autobilloprequest" || normSub == "autobill") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(1041)) return true;
            if ((normSub == "autobillop" || normSub == "autobilloprequest" || normSub == "autobill") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(1042)) return true;

            // Billing Categorization / Billing Categories (1043 - 1046)
            if ((normSub == "billingcategorization" || normSub == "billingcategories" || normSub == "opcategorycollection" || normSub == "categorycollection") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(1043)) return true;
            if ((normSub == "billingcategorization" || normSub == "billingcategories" || normSub == "opcategorycollection" || normSub == "categorycollection") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(1044)) return true;
            if ((normSub == "billingcategorization" || normSub == "billingcategories" || normSub == "opcategorycollection" || normSub == "categorycollection") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(1045)) return true;
            if ((normSub == "billingcategorization" || normSub == "billingcategories" || normSub == "opcategorycollection" || normSub == "categorycollection") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(1046)) return true;

            return false;
        }

        private bool CheckLaboratoryRoleAction(string normSub, string normAct)
        {
            // 1. Result Search (535 - 536)
            if ((normSub == "resultsearch" || normSub == "labresultsearch") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(535)) return true;
            if ((normSub == "resultsearch" || normSub == "labresultsearch") && (normAct == "print" || normAct == "printreport") && _permittedRoleIds.Contains(536)) return true;

            // 2. Result Entry (537 - 541)
            if ((normSub == "resultentry" || normSub == "labresultentry" || normSub == "labresultdataentry") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(537)) return true;
            if ((normSub == "resultentry" || normSub == "labresultentry" || normSub == "labresultdataentry") && (normAct == "create" || normAct == "add" || normAct == "save") && _permittedRoleIds.Contains(538)) return true;
            if ((normSub == "resultentry" || normSub == "labresultentry" || normSub == "labresultdataentry") && (normAct == "authorize" || normAct == "auth") && _permittedRoleIds.Contains(539)) return true;
            if ((normSub == "resultentry" || normSub == "labresultentry" || normSub == "labresultdataentry") && (normAct == "preview" || normAct == "previewreport") && _permittedRoleIds.Contains(540)) return true;
            if ((normSub == "resultentry" || normSub == "labresultentry" || normSub == "labresultdataentry") && (normAct == "print" || normAct == "printreport") && _permittedRoleIds.Contains(541)) return true;

            // 3. Test Master (542 - 545)
            if ((normSub == "testmaster" || normSub == "testmasters" || normSub == "tests") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(542)) return true;
            if ((normSub == "testmaster" || normSub == "testmasters" || normSub == "tests") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(543)) return true;
            if ((normSub == "testmaster" || normSub == "testmasters" || normSub == "tests") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(544)) return true;
            if ((normSub == "testmaster" || normSub == "testmasters" || normSub == "tests") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(545)) return true;

            // 4. Bill Items (546 - 549)
            if ((normSub == "billitems" || normSub == "billitem") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(546)) return true;
            if ((normSub == "billitems" || normSub == "billitem") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(547)) return true;
            if ((normSub == "billitems" || normSub == "billitem") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(548)) return true;
            if ((normSub == "billitems" || normSub == "billitem") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(549)) return true;

            // 5. Test Fee Master (550 - 552)
            if ((normSub == "testfeemaster" || normSub == "testfee") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(550)) return true;
            if ((normSub == "testfeemaster" || normSub == "testfee") && (normAct == "create" || normAct == "add" || normAct == "edit" || normAct == "update" || normAct == "save") && _permittedRoleIds.Contains(551)) return true;
            if ((normSub == "testfeemaster" || normSub == "testfee") && (normAct == "print" || normAct == "printreport") && _permittedRoleIds.Contains(552)) return true;

            // 6. Test Type Master (553 - 556)
            if ((normSub == "testtypemaster" || normSub == "testtypes") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(553)) return true;
            if ((normSub == "testtypemaster" || normSub == "testtypes") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(554)) return true;
            if ((normSub == "testtypemaster" || normSub == "testtypes") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(555)) return true;
            if ((normSub == "testtypemaster" || normSub == "testtypes") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(556)) return true;

            // 7. Group Master (557 - 560)
            if ((normSub == "groupmaster" || normSub == "groups") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(557)) return true;
            if ((normSub == "groupmaster" || normSub == "groups") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(558)) return true;
            if ((normSub == "groupmaster" || normSub == "groups") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(559)) return true;
            if ((normSub == "groupmaster" || normSub == "groups") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(560)) return true;

            // 8. Report Methods (561 - 564)
            if ((normSub == "reportmethods" || normSub == "reportmethodmaster" || normSub == "methodmaster") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(561)) return true;
            if ((normSub == "reportmethods" || normSub == "reportmethodmaster" || normSub == "methodmaster") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(562)) return true;
            if ((normSub == "reportmethods" || normSub == "reportmethodmaster" || normSub == "methodmaster") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(563)) return true;
            if ((normSub == "reportmethods" || normSub == "reportmethodmaster" || normSub == "methodmaster") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(564)) return true;

            // 9. Specimen Master (565 - 568)
            if ((normSub == "specimenmaster" || normSub == "samplemaster" || normSub == "samples") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(565)) return true;
            if ((normSub == "specimenmaster" || normSub == "samplemaster" || normSub == "samples") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(566)) return true;
            if ((normSub == "specimenmaster" || normSub == "samplemaster" || normSub == "samples") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(567)) return true;
            if ((normSub == "specimenmaster" || normSub == "samplemaster" || normSub == "samples") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(568)) return true;

            // 10. UOM Master (569 - 572)
            if ((normSub == "uommaster" || normSub == "unitmaster" || normSub == "units") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(569)) return true;
            if ((normSub == "uommaster" || normSub == "unitmaster" || normSub == "units") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(570)) return true;
            if ((normSub == "uommaster" || normSub == "unitmaster" || normSub == "units") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(571)) return true;
            if ((normSub == "uommaster" || normSub == "unitmaster" || normSub == "units") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(572)) return true;

            // 11. Machine Master (573 - 576)
            if ((normSub == "machinemaster" || normSub == "machines") && (normAct == "view" || normAct == "list") && _permittedRoleIds.Contains(573)) return true;
            if ((normSub == "machinemaster" || normSub == "machines") && (normAct == "create" || normAct == "add") && _permittedRoleIds.Contains(574)) return true;
            if ((normSub == "machinemaster" || normSub == "machines") && (normAct == "edit" || normAct == "update") && _permittedRoleIds.Contains(575)) return true;
            if ((normSub == "machinemaster" || normSub == "machines") && (normAct == "delete" || normAct == "cancel") && _permittedRoleIds.Contains(576)) return true;

            return false;
        }

        public bool HasRoleId(long roleId)
        {
            return _permittedRoleIds.Contains(roleId);
        }

        public bool HasRoleId(params long[] roleIds)
        {
            if (roleIds == null || roleIds.Length == 0) return false;
            return roleIds.Any(id => _permittedRoleIds.Contains(id));
        }

        public bool HasPermission(long roleId)
        {
            return _permittedRoleIds.Contains(roleId);
        }

        public bool HasPermission(params long[] roleIds)
        {
            if (roleIds == null || roleIds.Length == 0) return false;
            return roleIds.Any(id => _permittedRoleIds.Contains(id));
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
            int userCode = 0,
            bool isAdmin = false)
        {
            var result = new List<User_Rights>();
            var permList = permissions?.ToList() ?? new List<RolePermissionItem>();

            int sno = 1;
            int rightId = 1;

            var subGroups = permList
                .GroupBy(p => string.IsNullOrWhiteSpace(p.Sub_module) ? "General" : p.Sub_module.Trim())
                .ToList();

            bool isAdministrator = isAdmin;
            bool hasAnyMasterAdd = isAdministrator;
            bool hasAnyMasterEdit = isAdministrator;
            bool hasAnyMasterDelete = isAdministrator;

            foreach (var group in subGroups)
            {
                var subName = group.Key;
                var mainName = group.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Main_module))?.Main_module?.Trim() ?? "General";

                var actionNames = group
                    .Select(p => (p.Module ?? "").Trim().ToLowerInvariant())
                    .Where(a => !string.IsNullOrEmpty(a))
                    .ToList();

                bool toAdd = isAdministrator || actionNames.Any(a => 
                    a.Contains("create") || a.Contains("add") || a.Contains("new") || a.Contains("insert") || a.Contains("generate"));
                
                bool toEdit = isAdministrator || actionNames.Any(a => 
                    a.Contains("edit") || a.Contains("update") || a.Contains("modify") || a.Contains("change") || a.Contains("save"));

                bool toDelete = isAdministrator || actionNames.Any(a => 
                    a.Contains("delete") || a.Contains("remove") || a.Contains("cancel") || a.Contains("drop"));

                bool toExport = isAdministrator || actionNames.Any(a => 
                    a.Contains("download") || a.Contains("print") || a.Contains("export") || a.Contains("excel") || a.Contains("pdf"));

                bool toView = isAdministrator || actionNames.Any(a => 
                    a.Contains("view") || a.Contains("list") || a.Contains("dashboard") || a.Contains("report") || a.Contains("search") || a.Contains("read") || a.Contains("status") || a.Contains("preview") || a.Contains("display") || a.Contains("console") || a.Contains("notes")) || toAdd || toEdit || toDelete || toExport;

                if (toAdd) hasAnyMasterAdd = true;
                if (toEdit) hasAnyMasterEdit = true;
                if (toDelete) hasAnyMasterDelete = true;

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

            // Special page action mappings to ensure backward and cross-module compatibility across all pages
            // 1. Configuration / General (queried by 22+ master pages across 85 button checks)
            if (!result.Any(r => string.Equals(r.Department, "Configuration", StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, "General", StringComparison.OrdinalIgnoreCase)))
            {
                result.Add(new User_Rights
                {
                    UserModuleID = rightId,
                    Sno = sno++,
                    ModuleName = "General",
                    Department = "Configuration",
                    UserModuleRightsID = rightId++,
                    UserCode = userCode,
                    ToAdd = hasAnyMasterAdd,
                    ToView = true,
                    ToEdit = hasAnyMasterEdit,
                    ToDelete = hasAnyMasterDelete,
                    ToExport = true
                });
            }

            // 2. Reception / Refferal Case & Collection (queried by ReceptionBooking.razor & CustomerBookingHistory.razor)
            bool hasReceptionPerm = isAdministrator || permList.Any(p => 
                string.Equals(p.Main_module, "Reception", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Sub_module, "Patient check-in", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Sub_module, "Reception", StringComparison.OrdinalIgnoreCase));

            if (hasReceptionPerm && !result.Any(r => string.Equals(r.Department, "Reception", StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, "Refferal Case & Collection", StringComparison.OrdinalIgnoreCase)))
            {
                bool recepAdd = isAdministrator || permList.Any(p => p.Role_id == 368 || (p.Sub_module?.Contains("check-in", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                bool recepEdit = isAdministrator || permList.Any(p => p.Role_id == 369 || (p.Sub_module?.Contains("check-in", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0));
                bool recepDel = isAdministrator || permList.Any(p => p.Role_id == 370 || (p.Sub_module?.Contains("check-in", StringComparison.OrdinalIgnoreCase) == true && (p.Module?.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0 || p.Module?.IndexOf("Cancel", StringComparison.OrdinalIgnoreCase) >= 0)));
                bool recepView = isAdministrator || permList.Any(p => p.Role_id == 367 || (p.Sub_module?.Contains("check-in", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0)) || recepAdd || recepEdit || recepDel;

                result.Add(new User_Rights
                {
                    UserModuleID = rightId,
                    Sno = sno++,
                    ModuleName = "Refferal Case & Collection",
                    Department = "Reception",
                    UserModuleRightsID = rightId++,
                    UserCode = userCode,
                    ToAdd = recepAdd,
                    ToView = recepView,
                    ToEdit = recepEdit,
                    ToDelete = recepDel,
                    ToExport = true
                });
            }

            // 3. OP / OP Portal -> Patient Master (Roles 387-390)
            bool hasOpPatientPerm = isAdministrator || permList.Any(p =>
                (p.Role_id >= 387 && p.Role_id <= 390) ||
                string.Equals(p.Sub_module, "Patient Master", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Sub_module, "Patient Registration OP", StringComparison.OrdinalIgnoreCase));

            if (hasOpPatientPerm)
            {
                bool patAdd = isAdministrator || permList.Any(p => p.Role_id == 388 || (p.Sub_module?.Contains("Patient", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                bool patEdit = isAdministrator || permList.Any(p => p.Role_id == 389 || (p.Sub_module?.Contains("Patient", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0));
                bool patDel = isAdministrator || permList.Any(p => p.Role_id == 390 || (p.Sub_module?.Contains("Patient", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0));
                bool patView = isAdministrator || permList.Any(p => p.Role_id == 387 || (p.Sub_module?.Contains("Patient", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0)) || patAdd || patEdit || patDel;

                foreach (var dept in new[] { "OP Portal", "OP" })
                {
                    foreach (var mod in new[] { "Patient Master", "Patient Registration OP" })
                    {
                        if (!result.Any(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase)))
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = mod,
                                Department = dept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = patAdd,
                                ToView = patView,
                                ToEdit = patEdit,
                                ToDelete = patDel,
                                ToExport = true
                            });
                        }
                    }
                }
            }

            // 3a. Reception Masters: Country, State, City, Area, Service Type (Roles 391-410)
            var receptionMasters = new (int baseRoleId, string name)[]
            {
                (391, "Country Master"),
                (395, "State Master"),
                (399, "City Master"),
                (403, "Area Master"),
                (407, "Service Type Master")
            };

            foreach (var rm in receptionMasters)
            {
                bool hasRmPerm = isAdministrator || permList.Any(p =>
                    (p.Role_id >= rm.baseRoleId && p.Role_id <= rm.baseRoleId + 3) ||
                    p.Sub_module?.Equals(rm.name, StringComparison.OrdinalIgnoreCase) == true);

                if (hasRmPerm)
                {
                    bool rmView = isAdministrator || permList.Any(p => p.Role_id == rm.baseRoleId || (p.Sub_module?.Equals(rm.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool rmAdd = isAdministrator || permList.Any(p => p.Role_id == rm.baseRoleId + 1 || (p.Sub_module?.Equals(rm.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool rmEdit = isAdministrator || permList.Any(p => p.Role_id == rm.baseRoleId + 2 || (p.Sub_module?.Equals(rm.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool rmDel = isAdministrator || permList.Any(p => p.Role_id == rm.baseRoleId + 3 || (p.Sub_module?.Equals(rm.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0));

                    if (rmAdd || rmEdit || rmDel) rmView = true;

                    foreach (var dept in new[] { "OP Portal", "OP" })
                    {
                        if (!result.Any(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, rm.name, StringComparison.OrdinalIgnoreCase)))
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = rm.name,
                                Department = dept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = rmAdd,
                                ToView = rmView,
                                ToEdit = rmEdit,
                                ToDelete = rmDel,
                                ToExport = true
                            });
                        }
                    }
                }
            }

            // 4. Lab / Print Lab Result (queried by BillNoSequence.razor & BillsList.razor)
            bool hasLabPerm = isAdministrator || permList.Any(p => p.Main_module?.Contains("Lab", StringComparison.OrdinalIgnoreCase) == true);
            if (hasLabPerm && !result.Any(r => string.Equals(r.Department, "Lab", StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, "Print Lab Result", StringComparison.OrdinalIgnoreCase)))
            {
                bool labAdd = isAdministrator || permList.Any(p => p.Main_module?.Contains("Lab", StringComparison.OrdinalIgnoreCase) == true && (p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0 || p.Module?.IndexOf("Add", StringComparison.OrdinalIgnoreCase) >= 0));
                bool labEdit = isAdministrator || permList.Any(p => p.Main_module?.Contains("Lab", StringComparison.OrdinalIgnoreCase) == true && (p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0 || p.Module?.IndexOf("Authorize", StringComparison.OrdinalIgnoreCase) >= 0));
                bool labDel = isAdministrator || permList.Any(p => p.Main_module?.Contains("Lab", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0);

                result.Add(new User_Rights
                {
                    UserModuleID = rightId,
                    Sno = sno++,
                    ModuleName = "Print Lab Result",
                    Department = "Lab",
                    UserModuleRightsID = rightId++,
                    UserCode = userCode,
                    ToAdd = labAdd,
                    ToView = true,
                    ToEdit = labEdit,
                    ToDelete = labDel,
                    ToExport = true
                });
            }

            // 5. Billing / Billing Portal -> Counter Shift (queried by CounterShift.razor)
            bool hasShiftPerm = isAdministrator || permList.Any(p => p.Sub_module?.Contains("Counter Shift", StringComparison.OrdinalIgnoreCase) == true || p.Main_module?.Contains("Billing", StringComparison.OrdinalIgnoreCase) == true);
            if (hasShiftPerm)
            {
                bool shiftAdd = isAdministrator || permList.Any(p => p.Role_id == 510 || (p.Sub_module?.Contains("Counter Shift", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                bool shiftDel = isAdministrator || permList.Any(p => p.Role_id == 511 || (p.Sub_module?.Contains("Counter Shift", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0));
                bool shiftView = isAdministrator || permList.Any(p => p.Role_id == 509 || (p.Sub_module?.Contains("Counter Shift", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0)) || shiftAdd || shiftDel;

                foreach (var bDept in new[] { "Billing Portal", "Billing" })
                {
                    if (!result.Any(r => string.Equals(r.Department, bDept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, "Counter Shift", StringComparison.OrdinalIgnoreCase)))
                    {
                        result.Add(new User_Rights
                        {
                            UserModuleID = rightId,
                            Sno = sno++,
                            ModuleName = "Counter Shift",
                            Department = bDept,
                            UserModuleRightsID = rightId++,
                            UserCode = userCode,
                            ToAdd = shiftAdd,
                            ToView = shiftView,
                            ToEdit = shiftAdd,
                            ToDelete = shiftDel,
                            ToExport = true
                        });
                    }
                }
            }

            // 6. Billing / Billing Portal -> Due Collection & Due Collection History (Roles 491-496)
            bool hasDuePerm = isAdministrator || permList.Any(p => p.Sub_module?.Contains("Due Collection", StringComparison.OrdinalIgnoreCase) == true);
            if (hasDuePerm)
            {
                bool dueAdd = isAdministrator || permList.Any(p => p.Role_id == 492 || (p.Sub_module?.Equals("Due Collection", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                bool dueView = isAdministrator || permList.Any(p => p.Role_id == 491 || (p.Sub_module?.Equals("Due Collection", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0)) || dueAdd;

                bool histView = isAdministrator || permList.Any(p => p.Role_id == 493 || (p.Sub_module?.Equals("Due Collection History", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                bool histPrintPat = isAdministrator || permList.Any(p => p.Role_id == 494 || (p.Sub_module?.Equals("Due Collection History", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Print Patient", StringComparison.OrdinalIgnoreCase) >= 0));
                bool histPrintPay = isAdministrator || permList.Any(p => p.Role_id == 495 || (p.Sub_module?.Equals("Due Collection History", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Print Payment", StringComparison.OrdinalIgnoreCase) >= 0));
                bool histDel = isAdministrator || permList.Any(p => p.Role_id == 496 || (p.Sub_module?.Equals("Due Collection History", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0));

                foreach (var bDept in new[] { "Billing Portal", "Billing" })
                {
                    if (dueView || dueAdd)
                    {
                        var existingDue = result.FirstOrDefault(r => string.Equals(r.Department, bDept, StringComparison.OrdinalIgnoreCase) && 
                            (string.Equals(r.ModuleName, "Due Collection", StringComparison.OrdinalIgnoreCase) || 
                             string.Equals(r.ModuleName, "Due_Collection", StringComparison.OrdinalIgnoreCase) || 
                             string.Equals(r.ModuleName, "DueCollection", StringComparison.OrdinalIgnoreCase)));
                        if (existingDue != null)
                        {
                            existingDue.ToAdd = dueAdd;
                            existingDue.ToView = dueView;
                            existingDue.ToEdit = dueAdd;
                        }
                        else
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = "Due Collection",
                                Department = bDept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = dueAdd,
                                ToView = dueView,
                                ToEdit = dueAdd,
                                ToDelete = false,
                                ToExport = false
                            });
                        }
                    }

                    if (histView || histPrintPat || histPrintPay || histDel)
                    {
                        var existingHist = result.FirstOrDefault(r => string.Equals(r.Department, bDept, StringComparison.OrdinalIgnoreCase) && 
                            (string.Equals(r.ModuleName, "Due Collection History", StringComparison.OrdinalIgnoreCase) || 
                             string.Equals(r.ModuleName, "Due_Collection_History", StringComparison.OrdinalIgnoreCase) || 
                             string.Equals(r.ModuleName, "DueCollectionHistory", StringComparison.OrdinalIgnoreCase)));
                        if (existingHist != null)
                        {
                            existingHist.ToView = histView;
                            existingHist.ToExport = histPrintPat || histPrintPay;
                            existingHist.ToDelete = histDel;
                        }
                        else
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = "Due Collection History",
                                Department = bDept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = false,
                                ToView = histView,
                                ToEdit = false,
                                ToDelete = histDel,
                                ToExport = histPrintPat || histPrintPay
                            });
                        }
                    }
                }
            }

            // 7. Billing / Billing Portal -> Cash Counter (Roles 507, 508)
            bool hasCashCounterPerm = isAdministrator || permList.Any(p => p.Sub_module?.Contains("Cash Counter", StringComparison.OrdinalIgnoreCase) == true);
            if (hasCashCounterPerm)
            {
                bool ccAdd = isAdministrator || permList.Any(p => p.Role_id == 508 || (p.Sub_module?.Equals("Cash Counter", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                bool ccView = isAdministrator || permList.Any(p => p.Role_id == 507 || (p.Sub_module?.Equals("Cash Counter", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0)) || ccAdd;

                foreach (var bDept in new[] { "Billing Portal", "Billing" })
                {
                    if (ccView || ccAdd)
                    {
                        var existingCc = result.FirstOrDefault(r => string.Equals(r.Department, bDept, StringComparison.OrdinalIgnoreCase) && 
                            (string.Equals(r.ModuleName, "Cash Counter", StringComparison.OrdinalIgnoreCase) || 
                             string.Equals(r.ModuleName, "Cash_Counter", StringComparison.OrdinalIgnoreCase) || 
                             string.Equals(r.ModuleName, "CashCounter", StringComparison.OrdinalIgnoreCase)));
                        if (existingCc != null)
                        {
                            existingCc.ToAdd = ccAdd;
                            existingCc.ToView = ccView;
                            existingCc.ToEdit = ccAdd;
                        }
                        else
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = "Cash Counter",
                                Department = bDept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = ccAdd,
                                ToView = ccView,
                                ToEdit = ccAdd,
                                ToDelete = false,
                                ToExport = false
                            });
                        }
                    }
                }
            }

            // 7a. Billing Portal / Auto Bill OP (Roles 1039 - 1042)
            bool aboView = isAdministrator || permList.Any(p => p.Role_id == 1039 || ((string.Equals(p.Sub_module, "Auto Bill OP", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "AutoBill OP", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "AutoBillOpRequest", StringComparison.OrdinalIgnoreCase)) && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
            bool aboAdd = isAdministrator || permList.Any(p => p.Role_id == 1040 || ((string.Equals(p.Sub_module, "Auto Bill OP", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "AutoBill OP", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "AutoBillOpRequest", StringComparison.OrdinalIgnoreCase)) && (string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Module, "Add", StringComparison.OrdinalIgnoreCase))));
            bool aboEdit = isAdministrator || permList.Any(p => p.Role_id == 1041 || ((string.Equals(p.Sub_module, "Auto Bill OP", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "AutoBill OP", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "AutoBillOpRequest", StringComparison.OrdinalIgnoreCase)) && string.Equals(p.Module, "Edit", StringComparison.OrdinalIgnoreCase)));
            bool aboDel = isAdministrator || permList.Any(p => p.Role_id == 1042 || ((string.Equals(p.Sub_module, "Auto Bill OP", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "AutoBill OP", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "AutoBillOpRequest", StringComparison.OrdinalIgnoreCase)) && string.Equals(p.Module, "Delete", StringComparison.OrdinalIgnoreCase)));
            if (aboAdd || aboEdit || aboDel) aboView = true;

            foreach (var bDept in new[] { "Billing Portal", "Billing" })
            {
                foreach (var mod in new[] { "Auto Bill OP", "Auto_Bill_OP", "AutoBill OP", "AutoBillOP", "AutoBillOpRequest", "Auto Bill OP Request" })
                {
                    var existing = result.FirstOrDefault(r => string.Equals(r.Department, bDept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase));
                    if (existing != null)
                    {
                        existing.ToView = aboView;
                        existing.ToAdd = aboAdd;
                        existing.ToEdit = aboEdit;
                        existing.ToDelete = aboDel;
                        existing.ToExport = aboView;
                    }
                    else if (aboView || aboAdd || aboEdit || aboDel)
                    {
                        result.Add(new User_Rights
                        {
                            UserModuleID = rightId,
                            Sno = sno++,
                            ModuleName = mod,
                            Department = bDept,
                            UserModuleRightsID = rightId++,
                            UserCode = userCode,
                            ToAdd = aboAdd,
                            ToView = aboView,
                            ToEdit = aboEdit,
                            ToDelete = aboDel,
                            ToExport = true
                        });
                    }
                }
            }

            // 7b. Billing Portal / Billing Categorization (Roles 1043 - 1046)
            bool hasBillingCatPerm = isAdministrator || permList.Any(p =>
                (p.Role_id >= 1043 && p.Role_id <= 1046) ||
                string.Equals(p.Sub_module, "Billing Categorization", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Sub_module, "Billing Categories", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Sub_module, "OpCategoryCollection", StringComparison.OrdinalIgnoreCase));

            if (hasBillingCatPerm)
            {
                bool bcView = isAdministrator || permList.Any(p => p.Role_id == 1043 || ((string.Equals(p.Sub_module, "Billing Categorization", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "Billing Categories", StringComparison.OrdinalIgnoreCase)) && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                bool bcAdd = isAdministrator || permList.Any(p => p.Role_id == 1044 || ((string.Equals(p.Sub_module, "Billing Categorization", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "Billing Categories", StringComparison.OrdinalIgnoreCase)) && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                bool bcEdit = isAdministrator || permList.Any(p => p.Role_id == 1045 || ((string.Equals(p.Sub_module, "Billing Categorization", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "Billing Categories", StringComparison.OrdinalIgnoreCase)) && string.Equals(p.Module, "Edit", StringComparison.OrdinalIgnoreCase)));
                bool bcDel = isAdministrator || permList.Any(p => p.Role_id == 1046 || ((string.Equals(p.Sub_module, "Billing Categorization", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "Billing Categories", StringComparison.OrdinalIgnoreCase)) && string.Equals(p.Module, "Delete", StringComparison.OrdinalIgnoreCase)));
                if (bcAdd || bcEdit || bcDel) bcView = true;

                foreach (var bDept in new[] { "Billing Portal", "Billing" })
                {
                    foreach (var mod in new[] { "Billing Categorization", "Billing_Categorization", "BillingCategorization", "Billing Categories", "Billing_Categories", "BillingCategories", "OpCategoryCollection", "op-category-collection" })
                    {
                        var existing = result.FirstOrDefault(r => string.Equals(r.Department, bDept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase));
                        if (existing != null)
                        {
                            existing.ToView = bcView;
                            existing.ToAdd = bcAdd;
                            existing.ToEdit = bcEdit;
                            existing.ToDelete = bcDel;
                            existing.ToExport = true;
                        }
                        else
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = mod,
                                Department = bDept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = bcAdd,
                                ToView = bcView,
                                ToEdit = bcEdit,
                                ToDelete = bcDel,
                                ToExport = true
                            });
                        }
                    }
                }
            }

            // 8. IP Portal / IP Special Mappings (Roles 411 - 475)
            bool hasIpPortalPerm = isAdministrator || permList.Any(p => 
                (p.Role_id >= 411 && p.Role_id <= 475) ||
                string.Equals(p.Main_module, "IP Portal", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Main_module, "IP", StringComparison.OrdinalIgnoreCase));

            if (hasIpPortalPerm)
            {
                // 8a. IP Dashboard (Role 411)
                bool ipDashView = isAdministrator || permList.Any(p => p.Role_id == 411 || (p.Main_module?.Contains("IP", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Dashboard", StringComparison.OrdinalIgnoreCase) >= 0));
                if (ipDashView)
                {
                    foreach (var ipDept in new[] { "IP Portal", "IP" })
                    {
                        foreach (var dashMod in new[] { "IP Dashboard", "IP_Dashboard", "IPDashboard", "Dashboard" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, ipDept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, dashMod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = true;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = dashMod,
                                    Department = ipDept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = isAdministrator,
                                    ToView = true,
                                    ToEdit = isAdministrator,
                                    ToDelete = isAdministrator,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 8b. IP Registration (Roles 412 - 422)
                bool hasIpReg = isAdministrator || permList.Any(p => 
                    (p.Role_id >= 412 && p.Role_id <= 422) ||
                    p.Sub_module?.Equals("IP Registration", StringComparison.OrdinalIgnoreCase) == true ||
                    p.Sub_module?.Equals("Admission IP", StringComparison.OrdinalIgnoreCase) == true);

                if (hasIpReg)
                {
                    bool ipRegView = isAdministrator || permList.Any(p => p.Role_id == 412 || (p.Sub_module?.Equals("IP Registration", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                    bool ipRegAdd = isAdministrator || permList.Any(p => p.Role_id == 413 || (p.Sub_module?.Equals("IP Registration", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                    bool ipRegEdit = isAdministrator || permList.Any(p => p.Role_id == 414 || (p.Sub_module?.Equals("IP Registration", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Edit", StringComparison.OrdinalIgnoreCase)));
                    bool ipRegDel = isAdministrator || permList.Any(p => p.Role_id == 415 || (p.Sub_module?.Equals("IP Registration", StringComparison.OrdinalIgnoreCase) == true && (string.Equals(p.Module, "Delete", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Module, "Cancel", StringComparison.OrdinalIgnoreCase))));
                    bool ipRegPrev = isAdministrator || permList.Any(p => p.Role_id == 416 || (p.Sub_module?.Equals("IP Registration", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Preview", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool ipRegDown = isAdministrator || permList.Any(p => p.Role_id == 417 || (p.Sub_module?.Equals("IP Registration", StringComparison.OrdinalIgnoreCase) == true && (p.Module?.IndexOf("Download", StringComparison.OrdinalIgnoreCase) >= 0 || p.Module?.IndexOf("PDF", StringComparison.OrdinalIgnoreCase) >= 0)));
                    bool ipRegCaseView = isAdministrator || permList.Any(p => p.Role_id == 418 || (p.Sub_module?.Equals("IP Registration", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Casesheet", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool ipRegCasePrint = isAdministrator || permList.Any(p => p.Role_id == 419 || (p.Sub_module?.Equals("IP Registration", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Print Casesheet", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool ipRegDiscView = isAdministrator || permList.Any(p => p.Role_id == 420 || (p.Sub_module?.Equals("IP Registration", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View Discharge", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool ipRegDiscAdd = isAdministrator || permList.Any(p => p.Role_id == 421 || (p.Sub_module?.Equals("IP Registration", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create Discharge", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool ipRegDiscPrint = isAdministrator || permList.Any(p => p.Role_id == 422 || (p.Sub_module?.Equals("IP Registration", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Print Discharge", StringComparison.OrdinalIgnoreCase) >= 0));

                    if (ipRegAdd || ipRegEdit || ipRegDel || ipRegPrev || ipRegDown || ipRegCaseView || ipRegDiscView)
                    {
                        ipRegView = true;
                    }

                    foreach (var ipDept in new[] { "IP Portal", "IP" })
                    {
                        foreach (var regMod in new[] { "IP Registration", "IP_Registration", "IPRegistration", "Admission IP", "Inpatient Registration" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, ipDept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, regMod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = ipRegView;
                                existing.ToAdd = ipRegAdd;
                                existing.ToEdit = ipRegEdit;
                                existing.ToDelete = ipRegDel;
                                existing.ToExport = ipRegPrev || ipRegDown;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = regMod,
                                    Department = ipDept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = ipRegAdd,
                                    ToView = ipRegView,
                                    ToEdit = ipRegEdit,
                                    ToDelete = ipRegDel,
                                    ToExport = ipRegPrev || ipRegDown
                                });
                            }
                        }

                        // Case Sheet rights alias
                        foreach (var csMod in new[] { "Case Sheet", "Case_Sheet", "Casesheet", "View_CaseSheet" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, ipDept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, csMod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = ipRegCaseView;
                                existing.ToExport = ipRegCasePrint;
                            }
                            else if (ipRegCaseView || ipRegCasePrint)
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = csMod,
                                    Department = ipDept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = false,
                                    ToView = ipRegCaseView,
                                    ToEdit = false,
                                    ToDelete = false,
                                    ToExport = ipRegCasePrint
                                });
                            }
                        }
                    }
                }

                // 8c. Available Beds (Roles 423 - 425)
                bool hasAvailBeds = isAdministrator || permList.Any(p => 
                    (p.Role_id >= 423 && p.Role_id <= 425) ||
                    p.Sub_module?.Equals("Available Beds", StringComparison.OrdinalIgnoreCase) == true);
                if (hasAvailBeds)
                {
                    bool bView = isAdministrator || permList.Any(p => p.Role_id == 423 || (p.Sub_module?.Equals("Available Beds", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                    bool bAdd = isAdministrator || permList.Any(p => p.Role_id == 424 || (p.Sub_module?.Equals("Available Beds", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                    bool bEdit = isAdministrator || permList.Any(p => p.Role_id == 425 || (p.Sub_module?.Equals("Available Beds", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Edit", StringComparison.OrdinalIgnoreCase)));
                    if (bAdd || bEdit) bView = true;

                    foreach (var ipDept in new[] { "IP Portal", "IP" })
                    {
                        foreach (var bMod in new[] { "Available Beds", "Available_Beds", "AvailableBeds" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, ipDept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, bMod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = bView;
                                existing.ToAdd = bAdd;
                                existing.ToEdit = bEdit;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = bMod,
                                    Department = ipDept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = bAdd,
                                    ToView = bView,
                                    ToEdit = bEdit,
                                    ToDelete = false,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 8d. Bed Transfer (Roles 426 - 427)
                bool hasBedTransfer = isAdministrator || permList.Any(p => 
                    (p.Role_id >= 426 && p.Role_id <= 427) ||
                    p.Sub_module?.Equals("Bed Transfer", StringComparison.OrdinalIgnoreCase) == true);
                if (hasBedTransfer)
                {
                    bool tView = isAdministrator || permList.Any(p => p.Role_id == 426 || (p.Sub_module?.Equals("Bed Transfer", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                    bool tAdd = isAdministrator || permList.Any(p => p.Role_id == 427 || (p.Sub_module?.Equals("Bed Transfer", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                    if (tAdd) tView = true;

                    foreach (var ipDept in new[] { "IP Portal", "IP" })
                    {
                        foreach (var tMod in new[] { "Bed Transfer", "Bed_Transfer", "BedTransfer" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, ipDept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, tMod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = tView;
                                existing.ToAdd = tAdd;
                                existing.ToEdit = tAdd;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = tMod,
                                    Department = ipDept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = tAdd,
                                    ToView = tView,
                                    ToEdit = tAdd,
                                    ToDelete = false,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 8e. Discharge Summary (Roles 428 - 430)
                bool hasDiscSumm = isAdministrator || permList.Any(p => 
                    (p.Role_id >= 428 && p.Role_id <= 430) ||
                    p.Role_id == 420 || p.Role_id == 421 || p.Role_id == 422 ||
                    p.Sub_module?.Equals("Discharge Summary", StringComparison.OrdinalIgnoreCase) == true);
                if (hasDiscSumm)
                {
                    bool dsView = isAdministrator || permList.Any(p => p.Role_id == 428 || p.Role_id == 420 || (p.Sub_module?.Contains("Discharge", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool dsAdd = isAdministrator || permList.Any(p => p.Role_id == 429 || p.Role_id == 421 || (p.Sub_module?.Contains("Discharge", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool dsPrint = isAdministrator || permList.Any(p => p.Role_id == 430 || p.Role_id == 422 || (p.Sub_module?.Contains("Discharge", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Print", StringComparison.OrdinalIgnoreCase) >= 0));
                    if (dsAdd || dsPrint) dsView = true;

                    foreach (var ipDept in new[] { "IP Portal", "IP" })
                    {
                        foreach (var dsMod in new[] { "Discharge Summary", "Discharge_Summary", "DischargeSummary", "Discharge_Summery", "DischargeSummery" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, ipDept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, dsMod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = dsView;
                                existing.ToAdd = dsAdd;
                                existing.ToEdit = dsAdd;
                                existing.ToExport = dsPrint;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = dsMod,
                                    Department = ipDept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = dsAdd,
                                    ToView = dsView,
                                    ToEdit = dsAdd,
                                    ToDelete = false,
                                    ToExport = dsPrint
                                });
                            }
                        }
                    }
                }

                // 8f. Nurse Notes (Roles 431 - 435)
                bool hasNurse = isAdministrator || permList.Any(p => 
                    (p.Role_id >= 431 && p.Role_id <= 435) ||
                    p.Sub_module?.Equals("Nurse Notes", StringComparison.OrdinalIgnoreCase) == true);
                if (hasNurse)
                {
                    bool nView = isAdministrator || permList.Any(p => p.Role_id == 431 || (p.Sub_module?.Equals("Nurse Notes", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                    bool nAdd = isAdministrator || permList.Any(p => p.Role_id == 432 || (p.Sub_module?.Equals("Nurse Notes", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                    bool nEdit = isAdministrator || permList.Any(p => p.Role_id == 433 || (p.Sub_module?.Equals("Nurse Notes", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Edit", StringComparison.OrdinalIgnoreCase)));
                    bool nDel = isAdministrator || permList.Any(p => p.Role_id == 434 || (p.Sub_module?.Equals("Nurse Notes", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Delete", StringComparison.OrdinalIgnoreCase)));
                    bool nPrev = isAdministrator || permList.Any(p => p.Role_id == 435 || (p.Sub_module?.Equals("Nurse Notes", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Preview", StringComparison.OrdinalIgnoreCase) >= 0));
                    if (nAdd || nEdit || nDel || nPrev) nView = true;

                    foreach (var ipDept in new[] { "IP Portal", "IP" })
                    {
                        foreach (var nMod in new[] { "Nurse Notes", "Nurse_Notes", "NurseNotes" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, ipDept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, nMod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = nView;
                                existing.ToAdd = nAdd;
                                existing.ToEdit = nEdit;
                                existing.ToDelete = nDel;
                                existing.ToExport = nPrev;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = nMod,
                                    Department = ipDept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = nAdd,
                                    ToView = nView,
                                    ToEdit = nEdit,
                                    ToDelete = nDel,
                                    ToExport = nPrev
                                });
                            }
                        }
                    }
                }

                // 8g. 10 IP Masters (Roles 436 - 475)
                var ipMasters = new (int baseRoleId, string name)[]
                {
                    (436, "Bed Master"),
                    (440, "Ward Master"),
                    (444, "Room Type Master"),
                    (448, "Floor Master"),
                    (452, "Block Master"),
                    (456, "Nurse Master"),
                    (460, "IO Particulars Master"),
                    (464, "Service Name Master"),
                    (468, "Schedule Type Master"),
                    (472, "Discharge Template Master")
                };

                foreach (var im in ipMasters)
                {
                    bool hasImPerm = isAdministrator || permList.Any(p =>
                        (p.Role_id >= im.baseRoleId && p.Role_id <= im.baseRoleId + 3) ||
                        p.Sub_module?.Equals(im.name, StringComparison.OrdinalIgnoreCase) == true);

                    if (hasImPerm)
                    {
                        bool imView = isAdministrator || permList.Any(p => p.Role_id == im.baseRoleId || (p.Sub_module?.Equals(im.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                        bool imAdd = isAdministrator || permList.Any(p => p.Role_id == im.baseRoleId + 1 || (p.Sub_module?.Equals(im.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                        bool imEdit = isAdministrator || permList.Any(p => p.Role_id == im.baseRoleId + 2 || (p.Sub_module?.Equals(im.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0));
                        bool imDel = isAdministrator || permList.Any(p => p.Role_id == im.baseRoleId + 3 || (p.Sub_module?.Equals(im.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0));

                        if (imAdd || imEdit || imDel) imView = true;

                        var aliases = new List<string> { im.name, im.name.Replace(" ", "_"), im.name.Replace(" ", "") };
                        if (im.name == "Discharge Template Master")
                        {
                            aliases.AddRange(new[] { "Summary Templates", "Summary_Templates", "SummaryTemplates" });
                        }

                        foreach (var ipDept in new[] { "IP Portal", "IP" })
                        {
                            foreach (var aMod in aliases)
                            {
                                var existing = result.FirstOrDefault(r => string.Equals(r.Department, ipDept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                                if (existing != null)
                                {
                                    existing.ToView = imView;
                                    existing.ToAdd = imAdd;
                                    existing.ToEdit = imEdit;
                                    existing.ToDelete = imDel;
                                }
                                else
                                {
                                    result.Add(new User_Rights
                                    {
                                        UserModuleID = rightId,
                                        Sno = sno++,
                                        ModuleName = aMod,
                                        Department = ipDept,
                                        UserModuleRightsID = rightId++,
                                        UserCode = userCode,
                                        ToAdd = imAdd,
                                        ToView = imView,
                                        ToEdit = imEdit,
                                        ToDelete = imDel,
                                        ToExport = true
                                    });
                                }
                            }
                        }
                    }
                }
            }

            // 9. Inventory Special Mappings (Roles 590 - 653)
            bool hasInventoryPerm = isAdministrator || permList.Any(p =>
                (p.Role_id >= 590 && p.Role_id <= 653) ||
                string.Equals(p.Main_module, "Inventory", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Main_module, "Store", StringComparison.OrdinalIgnoreCase));

            if (hasInventoryPerm)
            {
                var invDepts = new[] { "Inventory", "Store" };

                // 9a. Purchase (Roles 590 - 593)
                bool hasPurchase = isAdministrator || permList.Any(p =>
                    (p.Role_id >= 590 && p.Role_id <= 593) ||
                    p.Sub_module?.Equals("Purchase", StringComparison.OrdinalIgnoreCase) == true ||
                    p.Sub_module?.Equals("Purchase Master", StringComparison.OrdinalIgnoreCase) == true);
                bool pView = isAdministrator || permList.Any(p => p.Role_id == 590 || ((string.Equals(p.Sub_module, "Purchase", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "Purchase Master", StringComparison.OrdinalIgnoreCase)) && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                bool pAdd = isAdministrator || permList.Any(p => p.Role_id == 591 || ((string.Equals(p.Sub_module, "Purchase", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "Purchase Master", StringComparison.OrdinalIgnoreCase)) && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                bool pEdit = isAdministrator || permList.Any(p => p.Role_id == 592 || ((string.Equals(p.Sub_module, "Purchase", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "Purchase Master", StringComparison.OrdinalIgnoreCase)) && string.Equals(p.Module, "Edit", StringComparison.OrdinalIgnoreCase)));
                bool pDel = isAdministrator || permList.Any(p => p.Role_id == 593 || ((string.Equals(p.Sub_module, "Purchase", StringComparison.OrdinalIgnoreCase) || string.Equals(p.Sub_module, "Purchase Master", StringComparison.OrdinalIgnoreCase)) && string.Equals(p.Module, "Delete", StringComparison.OrdinalIgnoreCase)));
                if (pAdd || pEdit || pDel) pView = true;

                if (hasPurchase)
                {

                    foreach (var dept in invDepts)
                    {
                        foreach (var mod in new[] { "Purchase", "Purchase_Purchase", "Purchase Purchase", "Purchase Master", "Purchase_Master", "Purchasemaster" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = pView;
                                existing.ToAdd = pAdd;
                                existing.ToEdit = pEdit;
                                existing.ToDelete = pDel;
                                existing.ToExport = true;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = mod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = pAdd,
                                    ToView = pView,
                                    ToEdit = pEdit,
                                    ToDelete = pDel,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 9aa. Dedicated Inventory / Purchase Order (Roles 1035 - 1038)
                bool hasExplicitPO = permList.Any(p => (p.Role_id >= 1035 && p.Role_id <= 1038) || string.Equals(p.Sub_module, "Purchase Order", StringComparison.OrdinalIgnoreCase));
                bool hasPurchaseOrder = isAdministrator || hasExplicitPO;
                if (hasPurchaseOrder)
                {
                    bool poView = isAdministrator || permList.Any(p => p.Role_id == 1035 || (string.Equals(p.Sub_module, "Purchase Order", StringComparison.OrdinalIgnoreCase) && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                    bool poAdd = isAdministrator || permList.Any(p => p.Role_id == 1036 || (string.Equals(p.Sub_module, "Purchase Order", StringComparison.OrdinalIgnoreCase) && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                    bool poEdit = isAdministrator || permList.Any(p => p.Role_id == 1037 || (string.Equals(p.Sub_module, "Purchase Order", StringComparison.OrdinalIgnoreCase) && string.Equals(p.Module, "Edit", StringComparison.OrdinalIgnoreCase)));
                    bool poDel = isAdministrator || permList.Any(p => p.Role_id == 1038 || (string.Equals(p.Sub_module, "Purchase Order", StringComparison.OrdinalIgnoreCase) && string.Equals(p.Module, "Delete", StringComparison.OrdinalIgnoreCase)));
                    if (poAdd || poEdit || poDel) poView = true;

                    foreach (var dept in invDepts)
                    {
                        foreach (var mod in new[] { "Purchase Order", "Purchase_Order", "PurchaseOrder" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = poView;
                                existing.ToAdd = poAdd;
                                existing.ToEdit = poEdit;
                                existing.ToDelete = poDel;
                                existing.ToExport = true;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = mod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = poAdd,
                                    ToView = poView,
                                    ToEdit = poEdit,
                                    ToDelete = poDel,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 9b. Purchase Return (Roles 594 - 595)
                bool hasPReturn = isAdministrator || permList.Any(p =>
                    (p.Role_id >= 594 && p.Role_id <= 595) ||
                    p.Sub_module?.Equals("Purchase Return", StringComparison.OrdinalIgnoreCase) == true);
                if (hasPReturn)
                {
                    bool prView = isAdministrator || permList.Any(p => p.Role_id == 594 || (p.Sub_module?.Contains("Purchase Return", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                    bool prAdd = isAdministrator || permList.Any(p => p.Role_id == 595 || (p.Sub_module?.Contains("Purchase Return", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                    if (prAdd) prView = true;

                    foreach (var dept in invDepts)
                    {
                        foreach (var mod in new[] { "Purchase Return", "Purchase_Return", "PurchaseReturn" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = prView;
                                existing.ToAdd = prAdd;
                                existing.ToEdit = prAdd;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = mod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = prAdd,
                                    ToView = prView,
                                    ToEdit = prAdd,
                                    ToDelete = false,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 9c. Purchase Payments (Roles 596 - 597)
                bool hasPPay = isAdministrator || permList.Any(p =>
                    (p.Role_id >= 596 && p.Role_id <= 597) ||
                    p.Sub_module?.Equals("Purchase Payments", StringComparison.OrdinalIgnoreCase) == true ||
                    p.Sub_module?.Equals("Purchase Payment", StringComparison.OrdinalIgnoreCase) == true);
                if (hasPPay)
                {
                    bool ppView = isAdministrator || permList.Any(p => p.Role_id == 596 || (p.Sub_module?.Contains("Purchase Payment", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                    bool ppAdd = isAdministrator || permList.Any(p => p.Role_id == 597 || (p.Sub_module?.Contains("Purchase Payment", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                    if (ppAdd) ppView = true;

                    foreach (var dept in invDepts)
                    {
                        foreach (var mod in new[] { "Purchase Payments", "Purchase_Payments", "PurchasePayments", "Purchase Payment", "Purchase_Payment", "PurchasePayment", "Purchase Filter", "Purchase_Filter", "PurchaseFilter" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = ppView;
                                existing.ToAdd = ppAdd;
                                existing.ToEdit = ppAdd;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = mod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = ppAdd,
                                    ToView = ppView,
                                    ToEdit = ppAdd,
                                    ToDelete = false,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 9d. Indent Request (Roles 598 - 601)
                bool hasIndentReq = isAdministrator || permList.Any(p =>
                    (p.Role_id >= 598 && p.Role_id <= 601) ||
                    p.Sub_module?.Equals("Indent Request", StringComparison.OrdinalIgnoreCase) == true);
                if (hasIndentReq)
                {
                    bool irView = isAdministrator || permList.Any(p => p.Role_id == 598 || (p.Sub_module?.Contains("Indent Request", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                    bool irAdd = isAdministrator || permList.Any(p => p.Role_id == 599 || (p.Sub_module?.Contains("Indent Request", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                    bool irEdit = isAdministrator || permList.Any(p => p.Role_id == 600 || (p.Sub_module?.Contains("Indent Request", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Edit", StringComparison.OrdinalIgnoreCase)));
                    bool irDel = isAdministrator || permList.Any(p => p.Role_id == 601 || (p.Sub_module?.Contains("Indent Request", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Delete", StringComparison.OrdinalIgnoreCase)));
                    if (irAdd || irEdit || irDel) irView = true;

                    foreach (var dept in invDepts)
                    {
                        foreach (var mod in new[] { "Indent Request", "Indent_Request", "IndentRequest" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = irView;
                                existing.ToAdd = irAdd;
                                existing.ToEdit = irEdit;
                                existing.ToDelete = irDel;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = mod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = irAdd,
                                    ToView = irView,
                                    ToEdit = irEdit,
                                    ToDelete = irDel,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 9e. Indent Approval (Roles 602 - 603)
                bool hasIndentAppr = isAdministrator || permList.Any(p =>
                    (p.Role_id >= 602 && p.Role_id <= 603) ||
                    p.Sub_module?.Equals("Indent Approval", StringComparison.OrdinalIgnoreCase) == true);
                if (hasIndentAppr)
                {
                    bool iaView = isAdministrator || permList.Any(p => p.Role_id == 602 || (p.Sub_module?.Contains("Indent Approval", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                    bool iaAdd = isAdministrator || permList.Any(p => p.Role_id == 603 || (p.Sub_module?.Contains("Indent Approval", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                    if (iaAdd) iaView = true;

                    foreach (var dept in invDepts)
                    {
                        foreach (var mod in new[] { "Indent Approval", "Indent_Approval", "IndentApproval" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = iaView;
                                existing.ToAdd = iaAdd;
                                existing.ToEdit = iaAdd;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = mod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = iaAdd,
                                    ToView = iaView,
                                    ToEdit = iaAdd,
                                    ToDelete = false,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 9f. Stock (Roles 604 - 606)
                bool hasStock = isAdministrator || permList.Any(p =>
                    (p.Role_id >= 604 && p.Role_id <= 606) ||
                    p.Sub_module?.Equals("Stock", StringComparison.OrdinalIgnoreCase) == true);
                if (hasStock)
                {
                    bool stView = isAdministrator || permList.Any(p => p.Role_id == 604 || (p.Sub_module?.Contains("Stock", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                    bool stPrev = isAdministrator || permList.Any(p => p.Role_id == 605 || (p.Sub_module?.Contains("Stock", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Preview", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool stPrint = isAdministrator || permList.Any(p => p.Role_id == 606 || (p.Sub_module?.Contains("Stock", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Print", StringComparison.OrdinalIgnoreCase) >= 0));
                    if (stPrev || stPrint) stView = true;

                    foreach (var dept in invDepts)
                    {
                        foreach (var mod in new[] { "Stock", "Stock_Stock", "Opening Stock", "Opening_Stock", "OpeningStock" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = stView;
                                existing.ToExport = stPrev || stPrint;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = mod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = isAdministrator,
                                    ToView = stView,
                                    ToEdit = isAdministrator,
                                    ToDelete = false,
                                    ToExport = stPrev || stPrint
                                });
                            }
                        }
                    }
                }

                // 9g. Stock Adjustment (Roles 607 - 608)
                bool hasStockAdj = isAdministrator || permList.Any(p =>
                    (p.Role_id >= 607 && p.Role_id <= 608) ||
                    p.Sub_module?.Equals("Stock Adjustment", StringComparison.OrdinalIgnoreCase) == true);
                if (hasStockAdj)
                {
                    bool saView = isAdministrator || permList.Any(p => p.Role_id == 607 || (p.Sub_module?.Contains("Stock Adjustment", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                    bool saAdd = isAdministrator || permList.Any(p => p.Role_id == 608 || (p.Sub_module?.Contains("Stock Adjustment", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                    if (saAdd) saView = true;

                    foreach (var dept in invDepts)
                    {
                        foreach (var mod in new[] { "Stock Adjustment", "Stock_Adjustment", "StockAdjustment" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = saView;
                                existing.ToAdd = saAdd;
                                existing.ToEdit = saAdd;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = mod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = saAdd,
                                    ToView = saView,
                                    ToEdit = saAdd,
                                    ToDelete = false,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 9h. Issue Stock (Roles 609 - 612)
                bool hasIssueStock = isAdministrator || permList.Any(p =>
                    (p.Role_id >= 609 && p.Role_id <= 612) ||
                    p.Sub_module?.Equals("Issue Stock", StringComparison.OrdinalIgnoreCase) == true);
                if (hasIssueStock)
                {
                    bool isView = isAdministrator || permList.Any(p => p.Role_id == 609 || (p.Sub_module?.Contains("Issue Stock", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                    bool isAdd = isAdministrator || permList.Any(p => p.Role_id == 610 || (p.Sub_module?.Contains("Issue Stock", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                    bool isEdit = isAdministrator || permList.Any(p => p.Role_id == 611 || (p.Sub_module?.Contains("Issue Stock", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Edit", StringComparison.OrdinalIgnoreCase)));
                    bool isDel = isAdministrator || permList.Any(p => p.Role_id == 612 || (p.Sub_module?.Contains("Issue Stock", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Delete", StringComparison.OrdinalIgnoreCase)));
                    if (isAdd || isEdit || isDel) isView = true;

                    foreach (var dept in invDepts)
                    {
                        foreach (var mod in new[] { "Issue Stock", "Issue_Stock", "IssueStock" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = isView;
                                existing.ToAdd = isAdd;
                                existing.ToEdit = isEdit;
                                existing.ToDelete = isDel;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = mod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = isAdd,
                                    ToView = isView,
                                    ToEdit = isEdit,
                                    ToDelete = isDel,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 9i. Receive Stock (Roles 613 - 614)
                bool hasRecStock = isAdministrator || permList.Any(p =>
                    (p.Role_id >= 613 && p.Role_id <= 614) ||
                    p.Sub_module?.Equals("Receive Stock", StringComparison.OrdinalIgnoreCase) == true);
                if (hasRecStock)
                {
                    bool rsView = isAdministrator || permList.Any(p => p.Role_id == 613 || (p.Sub_module?.Contains("Receive Stock", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                    bool rsAdd = isAdministrator || permList.Any(p => p.Role_id == 614 || (p.Sub_module?.Contains("Receive Stock", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                    if (rsAdd) rsView = true;

                    foreach (var dept in invDepts)
                    {
                        foreach (var mod in new[] { "Receive Stock", "Receive_Stock", "ReceiveStock" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = rsView;
                                existing.ToAdd = rsAdd;
                                existing.ToEdit = rsAdd;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = mod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = rsAdd,
                                    ToView = rsView,
                                    ToEdit = rsAdd,
                                    ToDelete = false,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 9j. Transfer Register (Role 615)
                bool hasTransfer = isAdministrator || permList.Any(p =>
                    p.Role_id == 615 ||
                    p.Sub_module?.Equals("Transfer Register", StringComparison.OrdinalIgnoreCase) == true ||
                    p.Sub_module?.Equals("Store Transfer", StringComparison.OrdinalIgnoreCase) == true);
                if (hasTransfer)
                {
                    bool trView = isAdministrator || permList.Any(p => p.Role_id == 615 || (p.Sub_module?.Contains("Transfer", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));

                    foreach (var dept in invDepts)
                    {
                        foreach (var mod in new[] { "Transfer Register", "Transfer_Register", "TransferRegister", "Store Transfer", "Store_Transfer", "StoreTransfer" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = trView;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = mod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = isAdministrator,
                                    ToView = trView,
                                    ToEdit = isAdministrator,
                                    ToDelete = false,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 9k. Doctor Consumption (Roles 616 - 617)
                bool hasDocCons = isAdministrator || permList.Any(p =>
                    (p.Role_id >= 616 && p.Role_id <= 617) ||
                    p.Sub_module?.Equals("Doctor Consumption", StringComparison.OrdinalIgnoreCase) == true);
                if (hasDocCons)
                {
                    bool dcView = isAdministrator || permList.Any(p => p.Role_id == 616 || (p.Sub_module?.Contains("Doctor Consumption", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "View", StringComparison.OrdinalIgnoreCase)));
                    bool dcAdd = isAdministrator || permList.Any(p => p.Role_id == 617 || (p.Sub_module?.Contains("Doctor Consumption", StringComparison.OrdinalIgnoreCase) == true && string.Equals(p.Module, "Create", StringComparison.OrdinalIgnoreCase)));
                    if (dcAdd) dcView = true;

                    foreach (var dept in invDepts)
                    {
                        foreach (var mod in new[] { "Doctor Consumption", "Doctor_Consumption", "DoctorConsumption" })
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, mod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = dcView;
                                existing.ToAdd = dcAdd;
                                existing.ToEdit = dcAdd;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = mod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = dcAdd,
                                    ToView = dcView,
                                    ToEdit = dcAdd,
                                    ToDelete = false,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 9l. 9 Inventory Masters (Roles 618 - 653)
                var invMasters = new (int baseRoleId, string name, string[] extraAliases)[]
                {
                    (618, "Item Master", new[] { "Item Master", "Item_Master", "ItemMaster", "Items" }),
                    (622, "Category Master", new[] { "Category Master", "Category_Master", "CategoryMaster", "Parent Category Master", "Parent_Category_Master", "ParentCategoryMaster" }),
                    (626, "Item Type Master", new[] { "Item Type Master", "Item_Type_Master", "ItemTypeMaster", "Sub Category Master", "Sub_Category_Master", "SubCategoryMaster" }),
                    (630, "Vendor Master", new[] { "Vendor Master", "Vendor_Master", "VendorMaster", "Vendors" }),
                    (634, "Unit Master", new[] { "Unit Master", "Unit_Master", "UnitMaster", "UOM Master", "UOM_Master", "UomMaster" }),
                    (638, "Store Master", new[] { "Store Master", "Store_Master", "StoreMaster", "Warehouse Master", "Warehouse_Master", "WarehouseMaster" }),
                    (642, "Manufacturer Master", new[] { "Manufacturer Master", "Manufacturer_Master", "ManufacturerMaster", "Manufacturers" }),
                    (646, "Ledger Group", new[] { "Ledger Group", "Ledger_Group", "LedgerGroup" }),
                    (650, "Ledger Type", new[] { "Ledger Type", "Ledger_Type", "LedgerType" })
                };

                foreach (var im in invMasters)
                {
                    bool hasImPerm = isAdministrator || permList.Any(p =>
                        (p.Role_id >= im.baseRoleId && p.Role_id <= im.baseRoleId + 3) ||
                        p.Sub_module?.Equals(im.name, StringComparison.OrdinalIgnoreCase) == true);

                    if (hasImPerm)
                    {
                        bool imView = isAdministrator || permList.Any(p => p.Role_id == im.baseRoleId || (p.Sub_module?.Equals(im.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                        bool imAdd = isAdministrator || permList.Any(p => p.Role_id == im.baseRoleId + 1 || (p.Sub_module?.Equals(im.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                        bool imEdit = isAdministrator || permList.Any(p => p.Role_id == im.baseRoleId + 2 || (p.Sub_module?.Equals(im.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0));
                        bool imDel = isAdministrator || permList.Any(p => p.Role_id == im.baseRoleId + 3 || (p.Sub_module?.Equals(im.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0));

                        if (imAdd || imEdit || imDel) imView = true;

                        foreach (var dept in invDepts)
                        {
                            foreach (var aMod in im.extraAliases)
                            {
                                var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                                if (existing != null)
                                {
                                    existing.ToView = imView;
                                    existing.ToAdd = imAdd;
                                    existing.ToEdit = imEdit;
                                    existing.ToDelete = imDel;
                                }
                                else
                                {
                                    result.Add(new User_Rights
                                    {
                                        UserModuleID = rightId,
                                        Sno = sno++,
                                        ModuleName = aMod,
                                        Department = dept,
                                        UserModuleRightsID = rightId++,
                                        UserCode = userCode,
                                        ToAdd = imAdd,
                                        ToView = imView,
                                        ToEdit = imEdit,
                                        ToDelete = imDel,
                                        ToExport = true
                                    });
                                }
                            }
                        }
                    }
                }

                // 10. Doctor Portal Special Mappings (Roles 654 - 684)
                var docDepts = new[] { "Doctor Portal", "Doctor", "Clinical" };

                // 10a. Doctor Patient List (Roles 654 - 657)
                bool hasDpl = isAdministrator || permList.Any(p => (p.Role_id >= 654 && p.Role_id <= 657) || p.Sub_module?.Equals("Doctor Patient List", StringComparison.OrdinalIgnoreCase) == true);
                if (hasDpl)
                {
                    bool dplView = isAdministrator || permList.Any(p => p.Role_id == 654 || (p.Sub_module?.Equals("Doctor Patient List", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool dplAdd = isAdministrator || permList.Any(p => p.Role_id == 655 || (p.Sub_module?.Equals("Doctor Patient List", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool dplEdit = isAdministrator || permList.Any(p => p.Role_id == 656 || (p.Sub_module?.Equals("Doctor Patient List", StringComparison.OrdinalIgnoreCase) == true && (p.Module?.IndexOf("Finalize", StringComparison.OrdinalIgnoreCase) >= 0 || p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0)));
                    bool dplPrint = isAdministrator || permList.Any(p => p.Role_id == 657 || (p.Sub_module?.Equals("Doctor Patient List", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Print", StringComparison.OrdinalIgnoreCase) >= 0));

                    if (dplAdd || dplEdit || dplPrint) dplView = true;

                    var dplAliases = new[] { "Doctor Patient List", "Doctor_Patient_List", "DoctorPatientList", "Patient List", "Doctor Appointments", "DoctorAppointments" };
                    foreach (var dept in docDepts)
                    {
                        foreach (var aMod in dplAliases)
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = dplView;
                                existing.ToAdd = dplAdd;
                                existing.ToEdit = dplEdit;
                                existing.ToExport = dplPrint;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = aMod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = dplAdd,
                                    ToView = dplView,
                                    ToEdit = dplEdit,
                                    ToDelete = false,
                                    ToExport = dplPrint
                                });
                            }
                        }
                    }
                }

                // 10b. Doctor Status (Roles 658 - 660)
                bool hasDs = isAdministrator || permList.Any(p => (p.Role_id >= 658 && p.Role_id <= 660) || p.Sub_module?.Equals("Doctor Status", StringComparison.OrdinalIgnoreCase) == true);
                if (hasDs)
                {
                    bool dsView = isAdministrator || permList.Any(p => p.Role_id == 658 || (p.Sub_module?.Equals("Doctor Status", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool dsAdd = isAdministrator || permList.Any(p => p.Role_id == 659 || (p.Sub_module?.Equals("Doctor Status", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool dsEdit = isAdministrator || permList.Any(p => p.Role_id == 660 || (p.Sub_module?.Equals("Doctor Status", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0));

                    if (dsAdd || dsEdit) dsView = true;

                    var dsAliases = new[] { "Doctor Status", "Doctor_Status", "DoctorStatus", "Doctor Current Status", "Doctor_Current_Status", "DoctorCurrentStatus" };
                    foreach (var dept in docDepts)
                    {
                        foreach (var aMod in dsAliases)
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = dsView;
                                existing.ToAdd = dsAdd;
                                existing.ToEdit = dsEdit;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = aMod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = dsAdd,
                                    ToView = dsView,
                                    ToEdit = dsEdit,
                                    ToDelete = false,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }

                // 10c. 6 Doctor Masters (Roles 661 - 684)
                var docMasters = new (int baseRoleId, string name, string[] extraAliases)[]
                {
                    (661, "Slot Master", new[] { "Slot Master", "Slot_Master", "SlotMaster", "Slots" }),
                    (665, "Doctors Master", new[] { "Doctors Master", "Doctors_Master", "Doctor Master", "Doctor_Master", "DoctorMaster", "Doctors" }),
                    (669, "Slot Type Master", new[] { "Slot Type Master", "Slot_Type_Master", "SlotTypeMaster", "Slot Types", "SlotTypes" }),
                    (673, "Doctor Type Master", new[] { "Doctor Type Master", "Doctor_Type_Master", "DoctorTypeMaster", "Doctor Types", "DoctorTypes" }),
                    (677, "Doctor Speciality Master", new[] { "Doctor Speciality Master", "Doctor_Speciality_Master", "DoctorSpecialityMaster", "Doctor Specialty Master", "Doctor_Specialty_Master", "DoctorSpecialtyMaster", "Doctor Specialties", "DoctorSpecialties" }),
                    (681, "Reimbursement Co", new[] { "Reimbursement Co", "Reimbursement_Co", "ReimbursementCo", "Reimbursement Company", "Reimbursement_Company", "Reimbursement Company Master", "ReimbursementCompanyMaster" })
                };

                foreach (var dm in docMasters)
                {
                    bool isSlotMaster = dm.baseRoleId == 661;
                    bool hasDmPerm = isAdministrator || permList.Any(p =>
                        (p.Role_id >= dm.baseRoleId && p.Role_id <= dm.baseRoleId + 3) ||
                        (isSlotMaster && p.Role_id >= 383 && p.Role_id <= 386) ||
                        p.Sub_module?.Equals(dm.name, StringComparison.OrdinalIgnoreCase) == true);

                    if (hasDmPerm)
                    {
                        bool dmView = isAdministrator || permList.Any(p =>
                            p.Role_id == dm.baseRoleId ||
                            (isSlotMaster && p.Role_id == 383) ||
                            (p.Sub_module?.Equals(dm.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));

                        bool dmAdd = isAdministrator || permList.Any(p =>
                            p.Role_id == dm.baseRoleId + 1 ||
                            (isSlotMaster && p.Role_id == 384) ||
                            (p.Sub_module?.Equals(dm.name, StringComparison.OrdinalIgnoreCase) == true && (p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0 || p.Module?.IndexOf("Add", StringComparison.OrdinalIgnoreCase) >= 0)));

                        bool dmEdit = isAdministrator || permList.Any(p =>
                            p.Role_id == dm.baseRoleId + 2 ||
                            (isSlotMaster && p.Role_id == 385) ||
                            (p.Sub_module?.Equals(dm.name, StringComparison.OrdinalIgnoreCase) == true && (p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0 || p.Module?.IndexOf("Update", StringComparison.OrdinalIgnoreCase) >= 0)));

                        bool dmDel = isAdministrator || permList.Any(p =>
                            p.Role_id == dm.baseRoleId + 3 ||
                            (isSlotMaster && p.Role_id == 386) ||
                            (p.Sub_module?.Equals(dm.name, StringComparison.OrdinalIgnoreCase) == true && (p.Module?.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0 || p.Module?.IndexOf("Cancel", StringComparison.OrdinalIgnoreCase) >= 0)));

                        if (dmAdd || dmEdit || dmDel) dmView = true;

                        var targetDepts = isSlotMaster
                            ? new[] { "Doctor Portal", "Doctor", "Clinical", "OP Portal", "OP", "Outpatient", "General" }
                            : docDepts;

                        foreach (var dept in targetDepts)
                        {
                            foreach (var aMod in dm.extraAliases)
                            {
                                var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                                if (existing != null)
                                {
                                    existing.ToView = dmView;
                                    existing.ToAdd = dmAdd;
                                    existing.ToEdit = dmEdit;
                                    existing.ToDelete = dmDel;
                                }
                                else
                                {
                                    result.Add(new User_Rights
                                    {
                                        UserModuleID = rightId,
                                        Sno = sno++,
                                        ModuleName = aMod,
                                        Department = dept,
                                        UserModuleRightsID = rightId++,
                                        UserCode = userCode,
                                        ToAdd = dmAdd,
                                        ToView = dmView,
                                        ToEdit = dmEdit,
                                        ToDelete = dmDel,
                                        ToExport = true
                                    });
                                }
                            }
                        }
                    }
                }
            }

            // 11. Vitals Special Mappings (Roles 685 - 708)
            var vitalsDepts = new[] { "Vitals", "Vital", "Clinical", "General" };

            // 11a. Vitals Dashboard (Role 685)
            bool hasVd = isAdministrator || permList.Any(p => p.Role_id == 685 || (p.Sub_module?.Equals("General", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Vitals Dashboard", StringComparison.OrdinalIgnoreCase) >= 0) || p.Sub_module?.IndexOf("Vitals Dashboard", StringComparison.OrdinalIgnoreCase) >= 0);
            if (hasVd)
            {
                var vdAliases = new[] { "Vitals Dashboard", "Vitals_Dashboard", "VitalsDashboard", "Dashboard" };
                foreach (var dept in vitalsDepts)
                {
                    foreach (var aMod in vdAliases)
                    {
                        var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                        if (existing != null)
                        {
                            existing.ToView = true;
                        }
                        else
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = aMod,
                                Department = dept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = false,
                                ToView = true,
                                ToEdit = false,
                                ToDelete = false,
                                ToExport = true
                            });
                        }
                    }
                }
            }

            // 11b. Token Display Screen (Role 696 / 382)
            bool hasTd = isAdministrator || permList.Any(p => p.Role_id == 696 || p.Role_id == 382 || p.Sub_module?.IndexOf("Token Display", StringComparison.OrdinalIgnoreCase) >= 0);
            if (hasTd)
            {
                var tdAliases = new[] { "Token Display Screen", "Token_Display_Screen", "TokenDisplayScreen", "Token Display", "Token_Display", "TokenDisplay", "OP Token Display", "OP_Token_Display", "OPTokenDisplay" };
                foreach (var dept in vitalsDepts)
                {
                    foreach (var aMod in tdAliases)
                    {
                        var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                        if (existing != null)
                        {
                            existing.ToView = true;
                        }
                        else
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = aMod,
                                Department = dept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = false,
                                ToView = true,
                                ToEdit = false,
                                ToDelete = false,
                                ToExport = true
                            });
                        }
                    }
                }
            }

            // 11c. Reception Portal (Roles 701 - 702)
            bool hasRp = isAdministrator || permList.Any(p => p.Role_id == 701 || p.Role_id == 702 || p.Sub_module?.IndexOf("Reception Portal", StringComparison.OrdinalIgnoreCase) >= 0);
            if (hasRp)
            {
                bool rpView = isAdministrator || permList.Any(p => p.Role_id == 701 || (p.Sub_module?.Equals("Reception Portal", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                bool rpAdd = isAdministrator || permList.Any(p => p.Role_id == 702 || (p.Sub_module?.Equals("Reception Portal", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                if (rpAdd) rpView = true;

                var rpAliases = new[] { "Reception Portal", "Reception_Portal", "ReceptionPortal", "Reception Booking", "Reception_Booking", "ReceptionBooking" };
                foreach (var dept in vitalsDepts)
                {
                    foreach (var aMod in rpAliases)
                    {
                        var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                        if (existing != null)
                        {
                            existing.ToView = rpView;
                            existing.ToAdd = rpAdd;
                        }
                        else
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = aMod,
                                Department = dept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = rpAdd,
                                ToView = rpView,
                                ToEdit = false,
                                ToDelete = false,
                                ToExport = true
                            });
                        }
                    }
                }
            }

            // 11d. OG Screen (Roles 703 - 704)
            bool hasOg = isAdministrator || permList.Any(p => p.Role_id == 703 || p.Role_id == 704 || p.Sub_module?.IndexOf("OG Screen", StringComparison.OrdinalIgnoreCase) >= 0);
            if (hasOg)
            {
                bool ogView = isAdministrator || permList.Any(p => p.Role_id == 703 || (p.Sub_module?.Equals("OG Screen", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                bool ogAdd = isAdministrator || permList.Any(p => p.Role_id == 704 || (p.Sub_module?.Equals("OG Screen", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                if (ogAdd) ogView = true;

                var ogAliases = new[] { "OG Screen", "OG_Screen", "OGScreen" };
                foreach (var dept in vitalsDepts)
                {
                    foreach (var aMod in ogAliases)
                    {
                        var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                        if (existing != null)
                        {
                            existing.ToView = ogView;
                            existing.ToAdd = ogAdd;
                        }
                        else
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = aMod,
                                Department = dept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = ogAdd,
                                ToView = ogView,
                                ToEdit = false,
                                ToDelete = false,
                                ToExport = true
                            });
                        }
                    }
                }
            }

            // 11e. Vitals Standard Submodules (Entry 686-689, LabTest 690-692, ScanResult 693-695, AppointPre 697-700, DoctorGroup 705-708)
            var vitalsSubmodules = new (int baseRoleId, int count, string name, string[] extraAliases)[]
            {
                (686, 4, "Vitals Entry", new[] { "Vitals Entry", "Vitals_Entry", "VitalsEntry", "Vitals", "Vital Entry", "Vital_Entry" }),
                (690, 3, "Lab Test Entry", new[] { "Lab Test Entry", "Lab_Test_Entry", "LabTestEntry", "Lab Test", "Lab_Test", "LabTest" }),
                (693, 3, "Scan Result Entry", new[] { "Scan Result Entry", "Scan_Result_Entry", "ScanResultEntry", "Scan Result", "Scan_Result", "ScanResult", "Scan Test Entry", "Scan_Test_Entry" }),
                (697, 4, "Appointment Pre-Booking", new[] { "Appointment Pre-Booking", "Appointment_Pre_Booking", "AppointmentPreBooking", "Appointment Pre Booking", "AppointPreBooking", "PreBooking" }),
                (705, 4, "Doctor Group Master", new[] { "Doctor Group Master", "Doctor_Group_Master", "DoctorGroupMaster", "Doctor Groups", "Doctor_Groups", "DoctorGroups", "Group Master", "GroupMaster" })
            };

            foreach (var vs in vitalsSubmodules)
            {
                bool hasVsPerm = isAdministrator || permList.Any(p =>
                    (p.Role_id >= vs.baseRoleId && p.Role_id < vs.baseRoleId + vs.count) ||
                    p.Sub_module?.Equals(vs.name, StringComparison.OrdinalIgnoreCase) == true);

                if (hasVsPerm)
                {
                    bool vsView = isAdministrator || permList.Any(p => p.Role_id == vs.baseRoleId || (p.Sub_module?.Equals(vs.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool vsAdd = isAdministrator || permList.Any(p => p.Role_id == vs.baseRoleId + 1 || (p.Sub_module?.Equals(vs.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool vsEdit = vs.count >= 3 && (isAdministrator || permList.Any(p => p.Role_id == vs.baseRoleId + 2 || (p.Sub_module?.Equals(vs.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0)));
                    bool vsDel = vs.count >= 4 && (isAdministrator || permList.Any(p => p.Role_id == vs.baseRoleId + 3 || (p.Sub_module?.Equals(vs.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0)));

                    if (vsAdd || vsEdit || vsDel) vsView = true;

                    foreach (var dept in vitalsDepts)
                    {
                        foreach (var aMod in vs.extraAliases)
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = vsView;
                                existing.ToAdd = vsAdd;
                                existing.ToEdit = vsEdit;
                                existing.ToDelete = vsDel;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = aMod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = vsAdd,
                                    ToView = vsView,
                                    ToEdit = vsEdit,
                                    ToDelete = vsDel,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }
            }

            // 12. Admin Portal Special Mappings (Roles 709 - 724)
            var adminDepts = new[] { "Admin Portal", "Admin", "Administration", "General" };

            // 12a. User Rights (Roles 713 - 714)
            bool hasUr = isAdministrator || permList.Any(p => p.Role_id == 713 || p.Role_id == 714 || p.Sub_module?.IndexOf("User Rights", StringComparison.OrdinalIgnoreCase) >= 0);
            if (hasUr)
            {
                bool urView = isAdministrator || permList.Any(p => p.Role_id == 713 || (p.Sub_module?.Equals("User Rights", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                bool urEdit = isAdministrator || permList.Any(p => p.Role_id == 714 || (p.Sub_module?.Equals("User Rights", StringComparison.OrdinalIgnoreCase) == true && (p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0 || p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0)));
                if (urEdit) urView = true;

                var urAliases = new[] { "User Rights", "User_Rights", "UserRights", "User Rights Template", "User_Rights_Template", "UserRightsTemplate", "User Master Rights", "User_Master_Rights", "UserMasterRights" };
                foreach (var dept in adminDepts)
                {
                    foreach (var aMod in urAliases)
                    {
                        var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                        if (existing != null)
                        {
                            existing.ToView = urView;
                            existing.ToAdd = urEdit;
                            existing.ToEdit = urEdit;
                        }
                        else
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = aMod,
                                Department = dept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = urEdit,
                                ToView = urView,
                                ToEdit = urEdit,
                                ToDelete = false,
                                ToExport = true
                            });
                        }
                    }
                }
            }

            // 12b. Settings (Roles 723 - 724)
            bool hasSet = isAdministrator || permList.Any(p => p.Role_id == 723 || p.Role_id == 724 || p.Sub_module?.IndexOf("Settings", StringComparison.OrdinalIgnoreCase) >= 0);
            if (hasSet)
            {
                bool setView = isAdministrator || permList.Any(p => p.Role_id == 723 || (p.Sub_module?.Equals("Settings", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                bool setAdd = isAdministrator || permList.Any(p => p.Role_id == 724 || (p.Sub_module?.Equals("Settings", StringComparison.OrdinalIgnoreCase) == true && (p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0 || p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0)));
                if (setAdd) setView = true;

                var setAliases = new[] { "Settings", "Lab Settings", "Lab_Settings", "LabSettings", "Report Settings", "Report_Settings", "ReportSettings" };
                foreach (var dept in adminDepts)
                {
                    foreach (var aMod in setAliases)
                    {
                        var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                        if (existing != null)
                        {
                            existing.ToView = setView;
                            existing.ToAdd = setAdd;
                            existing.ToEdit = setAdd;
                        }
                        else
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = aMod,
                                Department = dept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = setAdd,
                                ToView = setView,
                                ToEdit = setAdd,
                                ToDelete = false,
                                ToExport = true
                            });
                        }
                    }
                }
            }

            // 12c. Admin Masters (User Master 709-712, Product & Feature Registry 715-718, Branch Master 719-722)
            var adminMasters = new (int baseRoleId, string name, string[] extraAliases)[]
            {
                (709, "User Master", new[] { "User Master", "User_Master", "UserMaster", "Users" }),
                (715, "Product & Feature Registry", new[] { "Product & Feature Registry", "Product & Feature", "Product Features", "Product_Features", "ProductFeatures", "ProductFeatureRegistry" }),
                (719, "Branch Master", new[] { "Branch Master", "Branch_Master", "BranchMaster", "Branch", "Branches" })
            };

            foreach (var am in adminMasters)
            {
                bool hasAmPerm = isAdministrator || permList.Any(p =>
                    (p.Role_id >= am.baseRoleId && p.Role_id <= am.baseRoleId + 3) ||
                    p.Sub_module?.Equals(am.name, StringComparison.OrdinalIgnoreCase) == true);

                if (hasAmPerm)
                {
                    bool amView = isAdministrator || permList.Any(p => p.Role_id == am.baseRoleId || (p.Sub_module?.Equals(am.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool amAdd = isAdministrator || permList.Any(p => p.Role_id == am.baseRoleId + 1 || (p.Sub_module?.Equals(am.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool amEdit = isAdministrator || permList.Any(p => p.Role_id == am.baseRoleId + 2 || (p.Sub_module?.Equals(am.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool amDel = isAdministrator || permList.Any(p => p.Role_id == am.baseRoleId + 3 || (p.Sub_module?.Equals(am.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0));

                    if (amAdd || amEdit || amDel) amView = true;

                    foreach (var dept in adminDepts)
                    {
                        foreach (var aMod in am.extraAliases)
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = amView;
                                existing.ToAdd = amAdd;
                                existing.ToEdit = amEdit;
                                existing.ToDelete = amDel;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = aMod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = amAdd,
                                    ToView = amView,
                                    ToEdit = amEdit,
                                    ToDelete = amDel,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }
            }

            // 13. Billing Reports Special Mappings (Roles 727 - 728)
            bool hasRep = isAdministrator || permList.Any(p => p.Role_id == 727 || p.Role_id == 728 || p.Sub_module?.IndexOf("Reports", StringComparison.OrdinalIgnoreCase) >= 0);
            if (hasRep)
            {
                bool repPreview = isAdministrator || permList.Any(p => p.Role_id == 727 || (p.Sub_module?.Equals("Reports", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Preview", StringComparison.OrdinalIgnoreCase) >= 0));
                bool repPrint = isAdministrator || permList.Any(p => p.Role_id == 728 || (p.Sub_module?.Equals("Reports", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Print", StringComparison.OrdinalIgnoreCase) >= 0));

                var repAliases = new[] { "Reports", "Billing Reports", "Billing_Reports", "BillingReports" };
                var billDepts = new[] { "Billing Portal", "Billing", "Accounts" };
                foreach (var dept in billDepts)
                {
                    foreach (var aMod in repAliases)
                    {
                        var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                        if (existing != null)
                        {
                            existing.ToView = repPreview;
                            existing.ToExport = repPrint;
                        }
                        else
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = aMod,
                                Department = dept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = false,
                                ToView = repPreview,
                                ToEdit = false,
                                ToDelete = false,
                                ToExport = repPrint
                            });
                        }
                    }
                }
            }

            // 14. Laboratory Portal Special Mappings (Roles 535 - 576)
            var labDepts = new[] { "Laboratory", "Lab", "Diagnostics", "Clinical", "Configuration" };

            // 14a. Result Search (Roles 535 - 536)
            bool hasRs = isAdministrator || permList.Any(p => p.Role_id == 535 || p.Role_id == 536 || p.Sub_module?.IndexOf("Result Search", StringComparison.OrdinalIgnoreCase) >= 0);
            if (hasRs)
            {
                bool rsView = isAdministrator || permList.Any(p => p.Role_id == 535 || (p.Sub_module?.Equals("Result Search", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                bool rsPrint = isAdministrator || permList.Any(p => p.Role_id == 536 || (p.Sub_module?.Equals("Result Search", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Print", StringComparison.OrdinalIgnoreCase) >= 0));
                if (rsPrint) rsView = true;

                var rsAliases = new[] { "Result Search", "Result_Search", "ResultSearch", "Lab Result Search", "Lab_Result_Search", "LabResultSearch" };
                foreach (var dept in labDepts)
                {
                    foreach (var aMod in rsAliases)
                    {
                        var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                        if (existing != null)
                        {
                            existing.ToView = rsView;
                            existing.ToExport = rsPrint;
                        }
                        else
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = aMod,
                                Department = dept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = false,
                                ToView = rsView,
                                ToEdit = false,
                                ToDelete = false,
                                ToExport = rsPrint
                            });
                        }
                    }
                }
            }

            // 14b. Result Entry (Roles 537 - 541)
            bool hasRe = isAdministrator || permList.Any(p => (p.Role_id >= 537 && p.Role_id <= 541) || p.Sub_module?.IndexOf("Result Entry", StringComparison.OrdinalIgnoreCase) >= 0);
            if (hasRe)
            {
                bool reView = isAdministrator || permList.Any(p => p.Role_id == 537 || (p.Sub_module?.Equals("Result Entry", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                bool reAdd = isAdministrator || permList.Any(p => p.Role_id == 538 || (p.Sub_module?.Equals("Result Entry", StringComparison.OrdinalIgnoreCase) == true && (p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0 || p.Module?.IndexOf("Save", StringComparison.OrdinalIgnoreCase) >= 0)));
                bool reAuth = isAdministrator || permList.Any(p => p.Role_id == 539 || (p.Sub_module?.Equals("Result Entry", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Authorize", StringComparison.OrdinalIgnoreCase) >= 0));
                bool rePrev = isAdministrator || permList.Any(p => p.Role_id == 540 || (p.Sub_module?.Equals("Result Entry", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Preview", StringComparison.OrdinalIgnoreCase) >= 0));
                bool rePrint = isAdministrator || permList.Any(p => p.Role_id == 541 || (p.Sub_module?.Equals("Result Entry", StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Print", StringComparison.OrdinalIgnoreCase) >= 0));
                if (reAdd || reAuth || rePrev || rePrint) reView = true;

                var reAliases = new[] { "Result Entry", "Result_Entry", "ResultEntry", "Lab Result Entry", "Lab_Result_Entry", "LabResultEntry", "Lab Result Data Entry", "LabResultDataEntry", "Print Lab Result" };
                foreach (var dept in labDepts)
                {
                    foreach (var aMod in reAliases)
                    {
                        var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                        if (existing != null)
                        {
                            existing.ToView = reView;
                            existing.ToAdd = reAdd;
                            existing.ToEdit = reAuth;
                            existing.ToExport = rePrint || rePrev;
                        }
                        else
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = aMod,
                                Department = dept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = reAdd,
                                ToView = reView,
                                ToEdit = reAuth,
                                ToDelete = false,
                                ToExport = rePrint || rePrev
                            });
                        }
                    }
                }
            }

            // 14c. Test Fee Master (Roles 550 - 552)
            bool hasTf = isAdministrator || permList.Any(p => (p.Role_id >= 550 && p.Role_id <= 552) || p.Sub_module?.IndexOf("Test Fee Master", StringComparison.OrdinalIgnoreCase) >= 0 || p.Sub_module?.IndexOf("Test Fee", StringComparison.OrdinalIgnoreCase) >= 0);
            if (hasTf)
            {
                bool tfView = isAdministrator || permList.Any(p => p.Role_id == 550 || (p.Sub_module?.Contains("Test Fee") == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                bool tfAdd = isAdministrator || permList.Any(p => p.Role_id == 551 || (p.Sub_module?.Contains("Test Fee") == true && (p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0 || p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0)));
                bool tfPrint = isAdministrator || permList.Any(p => p.Role_id == 552 || (p.Sub_module?.Contains("Test Fee") == true && p.Module?.IndexOf("Print", StringComparison.OrdinalIgnoreCase) >= 0));
                if (tfAdd || tfPrint) tfView = true;

                var tfAliases = new[] { "Test Fee Master", "Test_Fee_Master", "TestFeeMaster", "Test Fee", "Test_Fee", "TestFee" };
                var tfDepts = new[] { "Laboratory", "Lab", "Billing Portal", "Billing", "Configuration" };
                foreach (var dept in tfDepts)
                {
                    foreach (var aMod in tfAliases)
                    {
                        var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                        if (existing != null)
                        {
                            existing.ToView = tfView;
                            existing.ToAdd = tfAdd;
                            existing.ToEdit = tfAdd;
                            existing.ToExport = tfPrint;
                        }
                        else
                        {
                            result.Add(new User_Rights
                            {
                                UserModuleID = rightId,
                                Sno = sno++,
                                ModuleName = aMod,
                                Department = dept,
                                UserModuleRightsID = rightId++,
                                UserCode = userCode,
                                ToAdd = tfAdd,
                                ToView = tfView,
                                ToEdit = tfAdd,
                                ToDelete = false,
                                ToExport = tfPrint
                            });
                        }
                    }
                }
            }

            // 14d. Laboratory Standard 4-Action Masters (Test Master 542-545, Bill Items 546-549, Test Type 553-556, Group Master 557-560, Report Methods 561-564, Specimen 565-568, UOM 569-572, Machine 573-576)
            var labMasters = new (int baseRoleId, string name, string[] extraAliases)[]
            {
                (542, "Test Master", new[] { "Test Master", "Test_Master", "TestMaster", "Tests", "Test Masters", "Test_Masters" }),
                (546, "Bill Items", new[] { "Bill Items", "Bill_Items", "BillItems", "Bill Item", "Bill_Item" }),
                (553, "Test Type Master", new[] { "Test Type Master", "Test_Type_Master", "TestTypeMaster", "Test Types", "Test_Types", "TestTypes" }),
                (557, "Group Master", new[] { "Group Master", "Group_Master", "GroupMaster", "Groups", "Lab Groups", "Lab_Groups" }),
                (561, "Report Methods", new[] { "Report Methods", "Report_Methods", "ReportMethods", "Report Method Master", "Report_Method_Master", "ReportMethodMaster", "Method Master", "Method_Master", "MethodMaster" }),
                (565, "Specimen Master", new[] { "Specimen Master", "Specimen_Master", "SpecimenMaster", "Sample Master", "Sample_Master", "SampleMaster", "Samples", "Specimens" }),
                (569, "UOM Master", new[] { "UOM Master", "UOM_Master", "UOMMaster", "Unit Master", "Unit_Master", "UnitMaster", "Units", "UOM", "UOMs" }),
                (573, "Machine Master", new[] { "Machine Master", "Machine_Master", "MachineMaster", "Machines" })
            };

            foreach (var lm in labMasters)
            {
                bool hasLmPerm = isAdministrator || permList.Any(p =>
                    (p.Role_id >= lm.baseRoleId && p.Role_id <= lm.baseRoleId + 3) ||
                    p.Sub_module?.Equals(lm.name, StringComparison.OrdinalIgnoreCase) == true);

                if (hasLmPerm)
                {
                    bool lmView = isAdministrator || permList.Any(p => p.Role_id == lm.baseRoleId || (p.Sub_module?.Equals(lm.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("View", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool lmAdd = isAdministrator || permList.Any(p => p.Role_id == lm.baseRoleId + 1 || (p.Sub_module?.Equals(lm.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Create", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool lmEdit = isAdministrator || permList.Any(p => p.Role_id == lm.baseRoleId + 2 || (p.Sub_module?.Equals(lm.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0));
                    bool lmDel = isAdministrator || permList.Any(p => p.Role_id == lm.baseRoleId + 3 || (p.Sub_module?.Equals(lm.name, StringComparison.OrdinalIgnoreCase) == true && p.Module?.IndexOf("Delete", StringComparison.OrdinalIgnoreCase) >= 0));

                    if (lmAdd || lmEdit || lmDel) lmView = true;

                    var targetDepts = lm.baseRoleId == 569 
                        ? new[] { "Laboratory", "Lab", "Diagnostics", "Inventory", "Store", "Configuration" }
                        : labDepts;

                    foreach (var dept in targetDepts)
                    {
                        foreach (var aMod in lm.extraAliases)
                        {
                            var existing = result.FirstOrDefault(r => string.Equals(r.Department, dept, StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, aMod, StringComparison.OrdinalIgnoreCase));
                            if (existing != null)
                            {
                                existing.ToView = lmView;
                                existing.ToAdd = lmAdd;
                                existing.ToEdit = lmEdit;
                                existing.ToDelete = lmDel;
                            }
                            else
                            {
                                result.Add(new User_Rights
                                {
                                    UserModuleID = rightId,
                                    Sno = sno++,
                                    ModuleName = aMod,
                                    Department = dept,
                                    UserModuleRightsID = rightId++,
                                    UserCode = userCode,
                                    ToAdd = lmAdd,
                                    ToView = lmView,
                                    ToEdit = lmEdit,
                                    ToDelete = lmDel,
                                    ToExport = true
                                });
                            }
                        }
                    }
                }
            }

            return result;
        }

        public static List<RolePermissionItem> ConvertUserRightsToPermissions(IEnumerable<User_Rights>? userRights)
        {
            var list = new List<RolePermissionItem>();
            if (userRights == null) return list;

            long roleId = 1;
            foreach (var r in userRights)
            {
                if (r == null) continue;
                if (!r.ToView && !r.ToAdd && !r.ToEdit && !r.ToDelete && !r.ToExport) continue;

                string main = !string.IsNullOrWhiteSpace(r.Department) ? r.Department.Trim() : "General";
                string sub = !string.IsNullOrWhiteSpace(r.ModuleName) ? r.ModuleName.Trim() : "General";

                string cleanSub = sub.Replace("_", " ");

                // Map department aliases
                var mainAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { main };
                var normMain = NormalizeKey(main);
                if (normMain == "op" || normMain == "reception" || normMain == "outpatient" || normMain == "opd") mainAliases.Add("OP Portal");
                if (normMain == "opportal") mainAliases.Add("OP");
                if (normMain == "ip" || normMain == "ipd" || normMain == "inpatient") mainAliases.Add("IP Portal");
                if (normMain == "ipportal") mainAliases.Add("IP");
                if (normMain == "ot" || normMain == "surgery") mainAliases.Add("Operation Theatre");
                if (normMain == "operationtheatre" || normMain == "operationtheater") mainAliases.Add("OT");
                if (normMain == "billing" || normMain == "cash" || normMain == "accounts") mainAliases.Add("Billing Portal");
                if (normMain == "billingportal") mainAliases.Add("Billing");
                if (normMain == "lab" || normMain == "diagnostics") mainAliases.Add("Laboratory");
                if (normMain == "laboratory") mainAliases.Add("Lab");
                if (normMain == "pharma") mainAliases.Add("Pharmacy");
                if (normMain == "pharmacy") mainAliases.Add("Pharma");
                if (normMain == "store" || normMain == "stock" || normMain == "purchase") mainAliases.Add("Inventory");
                if (normMain == "inventory") mainAliases.Add("Store");
                if (normMain == "doctor" || normMain == "doctors" || normMain == "clinical") mainAliases.Add("Doctor Portal");
                if (normMain == "doctorportal") mainAliases.Add("Doctor");
                if (normMain == "vital" || normMain == "vitalpage") mainAliases.Add("Vitals");
                if (normMain == "vitals") mainAliases.Add("Vital");
                if (normMain == "admin" || normMain == "administration") mainAliases.Add("Admin Portal");
                if (normMain == "adminportal") mainAliases.Add("Admin");

                // Map submodule aliases
                var subAliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { sub, cleanSub };
                var normSub = NormalizeKey(sub);
                if (normSub == "patientregistrationop" || normSub == "patientregistration") subAliases.Add("Patient Master");
                if (normSub == "patientmaster") subAliases.Add("Patient Registration OP");
                if (normSub == "refferalcasecollection" || normSub == "walkin") subAliases.Add("Patient Check-In");
                if (normSub == "patientcheckin") { subAliases.Add("Patient Check-in"); subAliases.Add("Walkin"); }
                if (normSub == "outpatientvisiting" || normSub == "opdconsultation" || normSub == "casesheet" || normSub == "viewcasesheet") subAliases.Add("Outpatient Visiting");
                if (normSub == "tokendisplay" || normSub == "tokendisplayscreen") { subAliases.Add("OP Token Display"); subAliases.Add("Token Display Screen"); }
                if (normSub == "optokendisplay") { subAliases.Add("Token Display Screen"); subAliases.Add("OP Token Display"); }
                if (normSub == "slots" || normSub == "slotmaster" || normSub == "slot") { subAliases.Add("Slot Master"); subAliases.Add("Slots"); }
                if (normSub == "servicetypemaster" || normSub == "servicetype") subAliases.Add("Service Type Master");
                if (normSub == "countrymaster" || normSub == "country") subAliases.Add("Country Master");
                if (normSub == "statemaster" || normSub == "state") subAliases.Add("State Master");
                if (normSub == "citymaster" || normSub == "city") subAliases.Add("City Master");
                if (normSub == "areamaster" || normSub == "area") subAliases.Add("Area Master");
                if (normSub == "ipregistration" || normSub == "admissionip") subAliases.Add("IP Registration");
                if (normSub == "availablebeds") { subAliases.Add("Available Beds"); subAliases.Add("Available Theaters"); }
                if (normSub == "bedtransfer") subAliases.Add("Bed Transfer");
                if (normSub == "dischargesummary" || normSub == "dischargesummery") subAliases.Add("Discharge Summary");
                if (normSub == "nursenotes") subAliases.Add("Nurse Notes");
                if (normSub == "summarytemplates" || normSub == "summerytemplates") subAliases.Add("Discharge Template Master");
                if (normSub == "dischargetemplatemaster") subAliases.Add("Summary Templates");
                if (normSub == "addot" || normSub == "otmaster") subAliases.Add("Add OT");
                if (normSub == "feecategories" || normSub == "otfeecategory") subAliases.Add("Fee Categories");
                if (normSub == "duecollection") subAliases.Add("Due Collection");
                if (normSub == "duecollectionhistory") subAliases.Add("Due Collection History");
                if (normSub == "cashcounter" || normSub == "cashcounterportal") subAliases.Add("Cash Counter");
                if (normSub == "countershift") subAliases.Add("Counter Shift");
                if (normSub == "doctorstatus") subAliases.Add("Doctor Status");
                if (normSub == "salesmaster" || normSub == "pharmacybilling") subAliases.Add("Pharmacy Billing");
                if (normSub == "purchasemaster" || normSub == "purchase") subAliases.Add("Purchase");
                if (normSub == "purchaseorder" || normSub == "purchase_order") subAliases.Add("Purchase Order");
                if (normSub == "purchasereturn") subAliases.Add("Purchase Return");
                if (normSub == "purchasefilter" || normSub == "purchasepayment" || normSub == "purchasepayments") subAliases.Add("Purchase Payments");
                if (normSub == "autobillop" || normSub == "autobilloprequest" || normSub == "autobill") { subAliases.Add("Auto Bill OP"); subAliases.Add("AutoBill OP"); }
                if (normSub == "billingcategorization" || normSub == "billingcategories" || normSub == "opcategorycollection") { subAliases.Add("Billing Categorization"); subAliases.Add("Billing Categories"); }
                if (normSub == "storetransfer" || normSub == "transferregister") subAliases.Add("Transfer Register");
                if (normSub == "billslist" || normSub == "bills" || normSub == "billlist") subAliases.Add("Bills List");
                if (normSub == "billingsummary" || normSub == "billsummary") subAliases.Add("Billing Summary");
                if (normSub == "billingreports" || normSub == "reports") subAliases.Add("Reports");
                if (normSub == "reimbursementcompanymaster" || normSub == "reimbursementco" || normSub == "reimbursementcompany") subAliases.Add("Reimbursement Co");
                if (normSub == "labsettings" || normSub == "settings") subAliases.Add("Settings");
                if (normSub == "usermaster") subAliases.Add("User Master");
                if (normSub == "userrights" || normSub == "userrightstemplate" || normSub == "usermasterrights") subAliases.Add("User Rights");
                if (normSub == "branchmaster") subAliases.Add("Branch Master");
                if (normSub == "productfeatures" || normSub == "productfeature" || normSub == "productfeatureregistry") subAliases.Add("Product & Feature Registry");

                foreach (var m in mainAliases)
                {
                    foreach (var s in subAliases)
                    {
                        if (r.ToView || r.ToAdd || r.ToEdit || r.ToDelete || r.ToExport)
                        {
                            list.Add(new RolePermissionItem { Role_id = roleId++, Main_module = m, Sub_module = s, Module = "View" });
                        }
                        if (r.ToAdd)
                        {
                            list.Add(new RolePermissionItem { Role_id = roleId++, Main_module = m, Sub_module = s, Module = "Create" });
                        }
                        if (r.ToEdit)
                        {
                            list.Add(new RolePermissionItem { Role_id = roleId++, Main_module = m, Sub_module = s, Module = "Edit" });
                        }
                        if (r.ToDelete)
                        {
                            list.Add(new RolePermissionItem { Role_id = roleId++, Main_module = m, Sub_module = s, Module = "Delete" });
                        }
                    }
                }
            }
            return list;
        }
    }
}
