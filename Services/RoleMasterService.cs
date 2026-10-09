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
        private readonly SharedComponents.Rcl.Services.TenantSessionState? _tenantState;

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

        public RoleMasterService(IHttpClientFactory clientFactory, SharedComponents.Rcl.Services.TenantSessionState? tenantState = null)
        {
            _clientFactory = clientFactory;
            _tenantState = tenantState;
        }

        private HttpClient Client => _clientFactory.CreateClient("MedicoAPI");

        private string ResolveTenantCode(string? explicitTenant = null)
        {
            if (!string.IsNullOrWhiteSpace(explicitTenant)) return explicitTenant.Trim();
            if (!string.IsNullOrWhiteSpace(_tenantState?.TenantCode)) return _tenantState.TenantCode.Trim();
            return "";
        }

        private void AttachTenantHeaders(HttpRequestMessage request, string? explicitTenant = null)
        {
            var effTenant = ResolveTenantCode(explicitTenant);
            if (!string.IsNullOrWhiteSpace(effTenant))
            {
                request.Headers.Remove("tenant_code");
                request.Headers.Add("tenant_code", effTenant);
                request.Headers.Remove("tenantcode");
                request.Headers.Add("tenantcode", effTenant);
                request.Headers.Remove("tenant-code");
                request.Headers.Add("tenant-code", effTenant);
            }
        }

        public string GetModuleIcon(string moduleName)
        {
            if (string.IsNullOrWhiteSpace(moduleName)) return "bi-folder2";
            return ModuleIcons.TryGetValue(moduleName, out var icon) ? icon : "bi-folder2";
        }

        public async Task<List<RolePermissionItem>> GetMasterCatalogAsync(string? tenantCode = null)
        {
            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(5));
                using var request = new HttpRequestMessage(HttpMethod.Get, "api/RoleMaster/master-roles?productId=MEDICO_APP");
                AttachTenantHeaders(request, tenantCode);
                var res = await Client.SendAsync(request, cts.Token);
                if (res.IsSuccessStatusCode)
                {
                    var response = await res.Content.ReadFromJsonAsync<List<RolePermissionItem>>(cts.Token);
                    if (response != null && response.Any())
                    {
                        return response;
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
                using var request = new HttpRequestMessage(HttpMethod.Get, "api/RoleMaster/get-roles");
                AttachTenantHeaders(request, tenantCode);
                var res = await Client.SendAsync(request);
                if (res.IsSuccessStatusCode)
                {
                    var response = await res.Content.ReadFromJsonAsync<List<RoleSummaryDto>>();
                    return response ?? new List<RoleSummaryDto>();
                }
                return new List<RoleSummaryDto>();
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
                using var request = new HttpRequestMessage(HttpMethod.Get, $"api/RoleMaster/get-role?roleGuid={roleGuid}");
                AttachTenantHeaders(request, tenantCode);
                var res = await Client.SendAsync(request);
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
                using var request = new HttpRequestMessage(HttpMethod.Post, "api/RoleMaster/save-role")
                {
                    Content = JsonContent.Create(req)
                };
                AttachTenantHeaders(request, tenantCode);
                var response = await Client.SendAsync(request);
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
                using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/RoleMaster/delete-role?roleGuid={roleGuid}");
                AttachTenantHeaders(request, tenantCode);
                var response = await Client.SendAsync(request);
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
                using var request = new HttpRequestMessage(HttpMethod.Get, $"api/RoleMaster/get-user-roles?usercode={usercode}");
                AttachTenantHeaders(request, tenantCode);
                var res = await Client.SendAsync(request);
                if (res.IsSuccessStatusCode)
                {
                    var response = await res.Content.ReadFromJsonAsync<List<long>>();
                    return response ?? new List<long>();
                }
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

            try
            {
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(10));
                using var request = new HttpRequestMessage(HttpMethod.Get, $"api/RoleMaster/get-user-effective-permissions?usercode={usercode}");
                AttachTenantHeaders(request, tenantCode);

                var res = await Client.SendAsync(request, cts.Token);
                if (res.IsSuccessStatusCode)
                {
                    var content = await res.Content.ReadAsStringAsync(cts.Token);
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        var list = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RolePermissionItem>>(content);
                        if (list != null)
                        {
                            Console.WriteLine($"[RoleMasterService] Successfully retrieved {list.Count} effective permissions for usercode {usercode} (tenant: {ResolveTenantCode(tenantCode)})");
                            return list;
                        }
                    }
                    return new List<RolePermissionItem>();
                }
                else
                {
                    Console.WriteLine($"[RoleMasterService] get-user-effective-permissions returned status code {res.StatusCode} for usercode {usercode}");
                }
            }
            catch (OperationCanceledException) { }
            catch (ObjectDisposedException) { }
            catch (Exception ex)
            {
                Console.WriteLine($"[RoleMasterService] Error fetching user effective permissions: {ex.Message}");
            }

            return new List<RolePermissionItem>();
        }

        public async Task<(bool Success, string Message)> SaveUserRolesAsync(long usercode, List<long> roleIds, string? tenantCode = null)
        {
            try
            {
                var payload = new { usercode, Role_ids = roleIds };
                using var request = new HttpRequestMessage(HttpMethod.Post, "api/RoleMaster/save-user-roles")
                {
                    Content = JsonContent.Create(payload)
                };
                AttachTenantHeaders(request, tenantCode);

                var effTenant = ResolveTenantCode(tenantCode);
                Console.WriteLine($"[RoleMasterService] Sending save-user-roles for usercode {usercode} with tenant_code: '{effTenant}' ({roleIds.Count} roles)");

                var response = await Client.SendAsync(request);
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
            if (norm == "adminportal" && (_permittedMainModules.Contains("admin") || _permittedMainModules.Contains("administration") || _permittedMainModules.Contains("adminportal") || _permittedMainModules.Contains("configuration") || _permittedMainModules.Contains("master"))) return true;

            return false;
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
                if (_permittedActions.Contains("opdashboard") || _permittedActions.Contains("dashboard") || _permittedSubModules.Contains("opdashboard"))
                    return true;
            }
            if (normSub == "ipdashboard")
            {
                if (_permittedActions.Contains("ipdashboard") || _permittedSubModules.Contains("ipdashboard"))
                    return true;
            }
            if (normSub == "patientcheckin" && (_permittedSubModules.Contains("walkin") || _permittedSubModules.Contains("patientcheckin") || _permittedSubModules.Contains("reception") || _permittedSubModules.Contains("refferalcasecollection"))) return true;
            if (normSub == "outpatientvisiting" && (_permittedSubModules.Contains("outpatientvisiting") || _permittedSubModules.Contains("outpatient") || _permittedSubModules.Contains("opdconsultation") || _permittedSubModules.Contains("casesheet") || _permittedSubModules.Contains("viewcasesheet"))) return true;
            if (normSub == "patientmaster" && (_permittedSubModules.Contains("patientmaster") || _permittedSubModules.Contains("patientregistrationop") || _permittedSubModules.Contains("patientregistration"))) return true;
            if (normSub == "countrymaster" && (_permittedSubModules.Contains("countrymaster") || _permittedSubModules.Contains("country"))) return true;
            if (normSub == "statemaster" && (_permittedSubModules.Contains("statemaster") || _permittedSubModules.Contains("state"))) return true;
            if (normSub == "citymaster" && (_permittedSubModules.Contains("citymaster") || _permittedSubModules.Contains("city"))) return true;
            if (normSub == "areamaster" && (_permittedSubModules.Contains("areamaster") || _permittedSubModules.Contains("area"))) return true;
            if (normSub == "servicetypemaster" && (_permittedSubModules.Contains("servicetypemaster") || _permittedSubModules.Contains("servicetype"))) return true;
            if (normSub == "slotmaster" && (_permittedSubModules.Contains("slotmaster") || _permittedSubModules.Contains("slots"))) return true;
            if ((normSub == "optokendisplay" || normSub == "tokendisplayscreen" || normSub == "tokendisplay") && (_permittedActions.Contains("viewtokendisplay") || _permittedSubModules.Contains("optokendisplay") || _permittedSubModules.Contains("tokendisplay") || _permittedSubModules.Contains("tokendisplayscreen"))) return true;

            // IP Portal
            if (normSub == "ipregistration" && (_permittedSubModules.Contains("ipregistration") || _permittedSubModules.Contains("admissionip"))) return true;
            if (normSub == "availabletheaters" && (_permittedSubModules.Contains("availabletheatre") || _permittedSubModules.Contains("availabletheater") || _permittedActions.Contains("availabletheaters"))) return true;
            if (normSub == "availabletheatre" && _permittedSubModules.Contains("availabletheaters")) return true;
            if ((normSub == "summarytemplates" || normSub == "dischargesummarytemplates" || normSub == "summerytemplates") && (_permittedSubModules.Contains("dischargetemplatemaster") || _permittedSubModules.Contains("summarytemplates") || _permittedSubModules.Contains("summerytemplates"))) return true;
            if (normSub == "dischargetemplatemaster" && (_permittedSubModules.Contains("summarytemplates") || _permittedSubModules.Contains("summerytemplates"))) return true;
            if (normSub == "dischargesummary" && (_permittedSubModules.Contains("dischargesummary") || _permittedSubModules.Contains("dischargesummery"))) return true;
            if (normSub == "otmaster" && (_permittedSubModules.Contains("addot") || _permittedActions.Contains("addot") || _permittedSubModules.Contains("otmaster"))) return true;
            if (normSub == "addot" && _permittedSubModules.Contains("otmaster")) return true;
            if (normSub == "doctorspecialtymaster" && (_permittedSubModules.Contains("doctorspecialitymaster") || _permittedSubModules.Contains("doctorspecialties") || _permittedSubModules.Contains("doctorspecialty"))) return true;
            if (normSub == "doctorspecialitymaster" && (_permittedSubModules.Contains("doctorspecialtymaster") || _permittedSubModules.Contains("doctorspecialties"))) return true;
            if (normSub == "itemtypemaster" && (_permittedSubModules.Contains("subcategorymaster") || _permittedSubModules.Contains("categorymaster") || _permittedSubModules.Contains("itemtypemaster"))) return true;
            if (normSub == "subcategorymaster" && _permittedSubModules.Contains("itemtypemaster")) return true;
            if (normSub == "purchasepayment" && (_permittedSubModules.Contains("purchasepayments") || _permittedSubModules.Contains("purchasepayment") || _permittedSubModules.Contains("purchasefilter"))) return true;
            if (normSub == "purchasepayments" && (_permittedSubModules.Contains("purchasepayment") || _permittedSubModules.Contains("purchasefilter"))) return true;
            if (normSub == "transferregister" && (_permittedSubModules.Contains("storetransfer") || _permittedSubModules.Contains("transferregister"))) return true;
            if (normSub == "storetransfer" && (_permittedSubModules.Contains("transferregister") || _permittedSubModules.Contains("storetransfer"))) return true;
            if (normSub == "billlist" && (_permittedSubModules.Contains("billslist") || _permittedSubModules.Contains("bills") || _permittedSubModules.Contains("billlist"))) return true;
            if (normSub == "billslist" && (_permittedSubModules.Contains("billlist") || _permittedSubModules.Contains("bills"))) return true;
            if (normSub == "billsummary" && (_permittedSubModules.Contains("billingsummary") || _permittedSubModules.Contains("billsummary"))) return true;
            if (normSub == "billingsummary" && (_permittedSubModules.Contains("billsummary") || _permittedSubModules.Contains("billingsummary"))) return true;
            if (normSub == "reimbursementco" && (_permittedSubModules.Contains("reimbursementcompanymaster") || _permittedSubModules.Contains("reimbursementcompany") || _permittedSubModules.Contains("reimbursements") || _permittedSubModules.Contains("reimbursementco"))) return true;
            if (normSub == "reimbursementcompany" && (_permittedSubModules.Contains("reimbursementco") || _permittedSubModules.Contains("reimbursementcompanymaster"))) return true;
            if (normSub == "settings" && (_permittedSubModules.Contains("settings") || _permittedSubModules.Contains("labsettings") || _permittedSubModules.Contains("reportsettings"))) return true;
            if (normSub == "labsettings" && (_permittedSubModules.Contains("settings") || _permittedSubModules.Contains("labsettings"))) return true;
            if (normSub == "userrights" && (_permittedSubModules.Contains("userrights") || _permittedSubModules.Contains("userrightstemplate") || _permittedSubModules.Contains("usermasterrights"))) return true;
            if (normSub == "productfeature" || normSub == "productfeatures" || normSub == "productfeatureregistry")
            {
                if (_permittedSubModules.Contains("productfeatures") || _permittedSubModules.Contains("productfeatureregistry") || _permittedSubModules.Contains("productfeature")) return true;
            }

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
            var permList = permissions?.ToList() ?? new List<RolePermissionItem>();

            int sno = 1;
            int rightId = 1;

            var subGroups = permList
                .GroupBy(p => string.IsNullOrWhiteSpace(p.Sub_module) ? "General" : p.Sub_module.Trim())
                .ToList();

            bool hasAnyAdd = userCode == 0 || userCode == 1;
            bool hasAnyEdit = userCode == 0 || userCode == 1;
            bool hasAnyDelete = userCode == 0 || userCode == 1;

            foreach (var group in subGroups)
            {
                var subName = group.Key;
                var mainName = group.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p.Main_module))?.Main_module?.Trim() ?? "General";

                var actionNames = group
                    .Select(p => (p.Module ?? "").Trim().ToLowerInvariant())
                    .Where(a => !string.IsNullOrEmpty(a))
                    .ToList();

                bool toAdd = hasAnyAdd || actionNames.Any(a => 
                    a.Contains("create") || a.Contains("add") || a.Contains("new") || a.Contains("insert") || a.Contains("generate"));
                
                bool toEdit = hasAnyEdit || actionNames.Any(a => 
                    a.Contains("edit") || a.Contains("update") || a.Contains("modify") || a.Contains("change") || a.Contains("save"));

                bool toDelete = hasAnyDelete || actionNames.Any(a => 
                    a.Contains("delete") || a.Contains("remove") || a.Contains("cancel") || a.Contains("drop"));

                bool toExport = actionNames.Any(a => 
                    a.Contains("download") || a.Contains("print") || a.Contains("export") || a.Contains("excel") || a.Contains("pdf")) || userCode == 0 || userCode == 1;

                bool toView = actionNames.Any(a => 
                    a.Contains("view") || a.Contains("list") || a.Contains("dashboard") || a.Contains("report") || a.Contains("search") || a.Contains("read") || a.Contains("status") || a.Contains("preview") || a.Contains("display") || a.Contains("console") || a.Contains("notes")) || toAdd || toEdit || toDelete || toExport || userCode == 0 || userCode == 1;

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

            // Special legacy page action mappings (only if matching submodule is explicitly permitted)
            if (permList.Any(p => p.Sub_module?.Contains("Patient Master", StringComparison.OrdinalIgnoreCase) == true || p.Sub_module?.Contains("Patient Registration", StringComparison.OrdinalIgnoreCase) == true))
            {
                if (!result.Any(r => string.Equals(r.Department, "OP", StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, "Patient Registration OP", StringComparison.OrdinalIgnoreCase)))
                {
                    result.Add(new User_Rights
                    {
                        UserModuleID = rightId,
                        Sno = sno++,
                        ModuleName = "Patient Registration OP",
                        Department = "OP",
                        UserModuleRightsID = rightId++,
                        UserCode = userCode,
                        ToAdd = hasAnyAdd,
                        ToView = true,
                        ToEdit = hasAnyEdit,
                        ToDelete = hasAnyDelete,
                        ToExport = true
                    });
                }
            }

            if (permList.Any(p => p.Sub_module?.Contains("Patient check-in", StringComparison.OrdinalIgnoreCase) == true || p.Sub_module?.Contains("Walkin", StringComparison.OrdinalIgnoreCase) == true || p.Sub_module?.Contains("Reception", StringComparison.OrdinalIgnoreCase) == true))
            {
                if (!result.Any(r => string.Equals(r.Department, "Reception", StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, "Refferal Case & Collection", StringComparison.OrdinalIgnoreCase)))
                {
                    result.Add(new User_Rights
                    {
                        UserModuleID = rightId,
                        Sno = sno++,
                        ModuleName = "Refferal Case & Collection",
                        Department = "Reception",
                        UserModuleRightsID = rightId++,
                        UserCode = userCode,
                        ToAdd = hasAnyAdd,
                        ToView = true,
                        ToEdit = hasAnyEdit,
                        ToDelete = hasAnyDelete,
                        ToExport = true
                    });
                }
            }

            if (permList.Any(p => p.Sub_module?.Contains("Result Entry", StringComparison.OrdinalIgnoreCase) == true || p.Sub_module?.Contains("Lab Result", StringComparison.OrdinalIgnoreCase) == true))
            {
                if (!result.Any(r => string.Equals(r.Department, "Lab", StringComparison.OrdinalIgnoreCase) && string.Equals(r.ModuleName, "Print Lab Result", StringComparison.OrdinalIgnoreCase)))
                {
                    result.Add(new User_Rights
                    {
                        UserModuleID = rightId,
                        Sno = sno++,
                        ModuleName = "Print Lab Result",
                        Department = "Lab",
                        UserModuleRightsID = rightId++,
                        UserCode = userCode,
                        ToAdd = hasAnyAdd,
                        ToView = true,
                        ToEdit = hasAnyEdit,
                        ToDelete = hasAnyDelete,
                        ToExport = true
                    });
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
                if (normMain == "admin" || normMain == "administration" || normMain == "configuration") mainAliases.Add("Admin Portal");
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
                if (normSub == "slots" || normSub == "slotmaster") subAliases.Add("Slot Master");
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
                if (normSub == "countershift") subAliases.Add("Counter Shift");
                if (normSub == "doctorstatus") subAliases.Add("Doctor Status");
                if (normSub == "salesmaster" || normSub == "pharmacybilling") subAliases.Add("Pharmacy Billing");
                if (normSub == "purchasemaster" || normSub == "purchase") subAliases.Add("Purchase");
                if (normSub == "purchaseorder") subAliases.Add("Purchase Order");
                if (normSub == "purchasereturn") subAliases.Add("Purchase Return");
                if (normSub == "purchasefilter" || normSub == "purchasepayment" || normSub == "purchasepayments") subAliases.Add("Purchase Payments");
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
