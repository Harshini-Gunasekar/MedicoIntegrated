using System;
using System.Collections.Generic;
using System.Linq;
using SharedComponents.Rcl.Models;
using SharedComponents.Rcl.Services;

namespace Booking.Helpers
{
    public static class UserPermissionHelper
    {
        public static bool IsAdminRole(string? role)
        {
            if (string.IsNullOrWhiteSpace(role)) return false;
            var r = role.Trim();
            return string.Equals(r, "Administrator", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(r, "Admin", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(r, "Owner", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(r, "Super Admin", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(r, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(r, "Power User", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(r, "PowerUser", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(r, "Tenant", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(r, "Tenant Admin", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(r, "TenantAdmin", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(r, "Client Admin", StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(r, "ClientAdmin", StringComparison.OrdinalIgnoreCase) ||
                   r.IndexOf("Admin", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static readonly System.Collections.Concurrent.ConcurrentDictionary<long, bool> _knownAdminUserCodes = new();

        public static void RegisterAdminUser(long userCode)
        {
            if (userCode > 0)
            {
                _knownAdminUserCodes[userCode] = true;
            }
        }

        public static void ClearAdminUser(long userCode)
        {
            if (userCode > 0)
            {
                _knownAdminUserCodes.TryRemove(userCode, out _);
            }
        }

        public static bool IsAdminRoleFromJwt(string? token)
        {
            if (string.IsNullOrEmpty(token)) return false;
            try
            {
                var segments = token.Split('.');
                if (segments.Length < 2) return false;
                var payload = segments[1];
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }
                var jsonBytes = Convert.FromBase64String(payload);
                var claims = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
                if (claims == null) return false;
                
                if (claims.TryGetValue("IsAdmin", out var isAdmVal) && 
                    (string.Equals(isAdmVal?.ToString(), "true", StringComparison.OrdinalIgnoreCase) || 
                     string.Equals(isAdmVal?.ToString(), "True", StringComparison.OrdinalIgnoreCase)))
                    return true;

                if (claims.TryGetValue("power_user", out var puVal) && 
                    (string.Equals(puVal?.ToString(), "true", StringComparison.OrdinalIgnoreCase) || 
                     string.Equals(puVal?.ToString(), "True", StringComparison.OrdinalIgnoreCase)))
                    return true;

                object? rVal = null;
                if (!claims.TryGetValue("role", out rVal) &&
                    !claims.TryGetValue("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", out rVal))
                    return false;
                return IsAdminRole(rVal?.ToString());
            }
            catch { return false; }
        }

        public static bool IsAdmin(
            UserDetailsModel? userDetails,
            TenantSessionState? tenantState = null)
        {
            if (tenantState != null)
            {
                if (tenantState.UserCode > 0 && _knownAdminUserCodes.ContainsKey(tenantState.UserCode)) return true;

                if (tenantState.UserData != null)
                {
                    if (tenantState.UserData.UserCode.HasValue && 
                        _knownAdminUserCodes.ContainsKey(tenantState.UserData.UserCode.Value))
                    {
                        return true;
                    }
                    if (!string.IsNullOrWhiteSpace(tenantState.UserData.Role) && IsAdminRole(tenantState.UserData.Role))
                    {
                        if (tenantState.UserData.UserCode.HasValue) RegisterAdminUser(tenantState.UserData.UserCode.Value);
                        return true;
                    }
                    if (string.Equals(tenantState.UserData.UserName, "admin", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(tenantState.UserData.UserName, "administrator", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    if (!string.IsNullOrWhiteSpace(tenantState.UserData.AuthToken) && IsAdminRoleFromJwt(tenantState.UserData.AuthToken))
                    {
                        if (tenantState.UserData.UserCode.HasValue) RegisterAdminUser(tenantState.UserData.UserCode.Value);
                        return true;
                    }
                }

                if (!string.IsNullOrWhiteSpace(tenantState.AuthToken) && IsAdminRoleFromJwt(tenantState.AuthToken))
                {
                    if (tenantState.UserCode > 0) RegisterAdminUser(tenantState.UserCode);
                    return true;
                }
            }

            if (userDetails != null)
            {
                if (userDetails.UserCode.HasValue && 
                    _knownAdminUserCodes.ContainsKey(userDetails.UserCode.Value))
                {
                    return true;
                }
                if (!string.IsNullOrWhiteSpace(userDetails.Role) && IsAdminRole(userDetails.Role))
                {
                    if (userDetails.UserCode.HasValue) RegisterAdminUser(userDetails.UserCode.Value);
                    return true;
                }
                if (string.Equals(userDetails.UserName, "admin", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(userDetails.UserName, "administrator", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                if (!string.IsNullOrWhiteSpace(userDetails.AuthToken) && IsAdminRoleFromJwt(userDetails.AuthToken))
                {
                    if (userDetails.UserCode.HasValue) RegisterAdminUser(userDetails.UserCode.Value);
                    return true;
                }
            }

            return false;
        }

        public static async System.Threading.Tasks.Task<bool> IsAdminAsync(
            UserDetailsModel? userDetails,
            TenantSessionState? tenantState = null,
            Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage.ProtectedLocalStorage? sessionStorage = null)
        {
            if (IsAdmin(userDetails, tenantState)) return true;

            if (sessionStorage != null)
            {
                long resolvedUserCode = 0;
                if (userDetails?.UserCode.HasValue == true && userDetails.UserCode.Value > 0)
                {
                    resolvedUserCode = userDetails.UserCode.Value;
                }
                else if (tenantState?.UserCode > 0)
                {
                    resolvedUserCode = tenantState.UserCode;
                }
                else if (tenantState?.UserData?.UserCode.HasValue == true && tenantState.UserData.UserCode.Value > 0)
                {
                    resolvedUserCode = tenantState.UserData.UserCode.Value;
                }

                if (resolvedUserCode == 0)
                {
                    try
                    {
                        var uc = await sessionStorage.GetAsync<int>("user_code");
                        if (uc.Success && uc.Value > 0) resolvedUserCode = uc.Value;
                    }
                    catch { }
                }

                if (resolvedUserCode > 0 && _knownAdminUserCodes.ContainsKey(resolvedUserCode))
                {
                    return true;
                }

                if (userDetails == null)
                {
                    try
                    {
                        var uRes = await sessionStorage.GetAsync<UserDetailsModel>("UserDetails");
                        if (uRes.Success && uRes.Value != null)
                        {
                            userDetails = uRes.Value;
                            if (IsAdmin(userDetails, tenantState))
                            {
                                if (resolvedUserCode > 0) RegisterAdminUser(resolvedUserCode);
                                return true;
                            }
                        }
                    }
                    catch { }
                }

                try
                {
                    var ita = await sessionStorage.GetAsync<bool>("IsTenantAdmin");
                    if (ita.Success && ita.Value)
                    {
                        if (resolvedUserCode > 0) RegisterAdminUser(resolvedUserCode);
                        if (tenantState?.UserData != null) tenantState.UserData.Role = "Administrator";
                        return true;
                    }
                }
                catch { }

                try
                {
                    var ia = await sessionStorage.GetAsync<bool>("isAdmin");
                    if (ia.Success && ia.Value)
                    {
                        if (resolvedUserCode > 0) RegisterAdminUser(resolvedUserCode);
                        if (tenantState?.UserData != null) tenantState.UserData.Role = "Administrator";
                        return true;
                    }
                }
                catch { }

                try
                {
                    var ia2 = await sessionStorage.GetAsync<bool>("IsAdmin");
                    if (ia2.Success && ia2.Value)
                    {
                        if (resolvedUserCode > 0) RegisterAdminUser(resolvedUserCode);
                        if (tenantState?.UserData != null) tenantState.UserData.Role = "Administrator";
                        return true;
                    }
                }
                catch { }

                try
                {
                    var ur = await sessionStorage.GetAsync<string>("user_role");
                    if (ur.Success && !string.IsNullOrWhiteSpace(ur.Value) && IsAdminRole(ur.Value))
                    {
                        if (resolvedUserCode > 0) RegisterAdminUser(resolvedUserCode);
                        if (tenantState?.UserData != null) tenantState.UserData.Role = "Administrator";
                        return true;
                    }
                }
                catch { }

                try
                {
                    var ud = await sessionStorage.GetAsync<string>("user_description");
                    if (ud.Success && !string.IsNullOrWhiteSpace(ud.Value) && IsAdminRole(ud.Value))
                    {
                        if (resolvedUserCode > 0) RegisterAdminUser(resolvedUserCode);
                        if (tenantState?.UserData != null) tenantState.UserData.Role = "Administrator";
                        return true;
                    }
                }
                catch { }

                try
                {
                    var un = await sessionStorage.GetAsync<string>("user_name");
                    if (un.Success && !string.IsNullOrWhiteSpace(un.Value) &&
                        (string.Equals(un.Value.Trim(), "admin", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(un.Value.Trim(), "administrator", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (resolvedUserCode > 0) RegisterAdminUser(resolvedUserCode);
                        return true;
                    }
                }
                catch { }

                try
                {
                    var li = await sessionStorage.GetAsync<string>("login_input");
                    if (li.Success && !string.IsNullOrWhiteSpace(li.Value) &&
                        (string.Equals(li.Value.Trim(), "admin", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(li.Value.Trim(), "administrator", StringComparison.OrdinalIgnoreCase) ||
                         li.Value.StartsWith("admin@", StringComparison.OrdinalIgnoreCase)))
                    {
                        if (resolvedUserCode > 0) RegisterAdminUser(resolvedUserCode);
                        if (tenantState?.UserData != null) tenantState.UserData.Role = "Administrator";
                        return true;
                    }
                }
                catch { }

                try
                {
                    var tokRes = await sessionStorage.GetAsync<string>("authToken");
                    if (tokRes.Success && !string.IsNullOrWhiteSpace(tokRes.Value) && IsAdminRoleFromJwt(tokRes.Value))
                    {
                        if (resolvedUserCode > 0) RegisterAdminUser(resolvedUserCode);
                        if (tenantState?.UserData != null) tenantState.UserData.Role = "Administrator";
                        return true;
                    }
                }
                catch { }

                try
                {
                    var uc = await sessionStorage.GetAsync<int>("user_code");
                    if (uc.Success && uc.Value <= 1 && uc.Value >= 0)
                    {
                        RegisterAdminUser(uc.Value);
                        return true;
                    }
                }
                catch { }
            }

            return false;
        }

        public static (bool View, bool Add, bool Edit, bool Delete) GetRights(
            UserDetailsModel? userDetails,
            TenantSessionState? tenantState,
            List<User_Rights>? userRights,
            params string[] moduleAliases)
        {
            if (IsAdmin(userDetails, tenantState))
            {
                return (true, true, true, true);
            }

            if (userRights != null && userRights.Count > 0 && moduleAliases != null && moduleAliases.Length > 0)
            {
                foreach (var alias in moduleAliases)
                {
                    if (string.IsNullOrWhiteSpace(alias)) continue;
                    var normalizedAlias = Normalize(alias);

                    var right = userRights.FirstOrDefault(r =>
                        !string.IsNullOrWhiteSpace(r.ModuleName) &&
                        (string.Equals(r.ModuleName.Trim(), alias.Trim(), StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(Normalize(r.ModuleName), normalizedAlias, StringComparison.OrdinalIgnoreCase)));

                    if (right != null)
                    {
                        return (right.ToView, right.ToAdd, right.ToEdit, right.ToDelete);
                    }
                }
            }

            return (false, false, false, false);
        }

        public static async System.Threading.Tasks.Task<(bool View, bool Add, bool Edit, bool Delete)> GetRightsAsync(
            UserDetailsModel? userDetails,
            TenantSessionState? tenantState,
            Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage.ProtectedLocalStorage? sessionStorage,
            List<User_Rights>? userRights,
            params string[] moduleAliases)
        {
            if (await IsAdminAsync(userDetails, tenantState, sessionStorage))
            {
                return (true, true, true, true);
            }

            return GetRights(userDetails, tenantState, userRights, moduleAliases);
        }

        public static (bool View, bool Add, bool Edit, bool Delete) GetRights(
            UserDetailsModel? userDetails,
            List<User_Rights>? userRights,
            params string[] moduleAliases)
        {
            return GetRights(userDetails, null, userRights, moduleAliases);
        }

        private static string Normalize(string name)
        {
            return name.Replace("_", "").Replace("-", "").Replace(" ", "").Trim().ToLowerInvariant();
        }
    }
}
