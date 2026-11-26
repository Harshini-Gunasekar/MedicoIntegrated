using System;
using System.Collections.Generic;
using System.Linq;
using SharedComponents.Rcl.Models;

namespace SharedComponents.Rcl.Services;

/// <summary>
/// Scoped state container that holds the active tenant's session data.
/// Populated by the host app's MainLayout when the session is loaded from ProtectedSessionStorage.
/// Consumed by components and HTTP handlers to inject request headers automatically.
/// </summary>
public class TenantSessionState
{
    public string TenantCode { get; private set; } = string.Empty;
    public string AuthToken { get; private set; } = string.Empty;
    public string TenantName { get; private set; } = string.Empty;
    public int UserCode { get; private set; } = 0;
    public int? BranchCode { get; private set; }
    public int? CounterCode { get; private set; }

    public UserDetailsModel? UserData { get; private set; }
    public IList<User_Rights> UserRightsList { get; private set; } = new List<User_Rights>();

    public bool IsAuthenticated => !string.IsNullOrEmpty(AuthToken);

    public event Action? OnSessionChanged;

    public bool IsAdmin =>
        UserCode == 0 || UserCode == 1 ||
        UserData == null ||
        string.IsNullOrEmpty(UserData.Role) ||
        string.Equals(UserData.Role, "Administrator", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(UserData.Role, "Admin", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(UserData.Role, "Tenant", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(UserData.Role, "Power User", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(UserData.Role, "PowerUser", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(UserData.Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(UserData.Role, "Super Admin", StringComparison.OrdinalIgnoreCase) ||
        (UserData.Role?.Contains("Admin", StringComparison.OrdinalIgnoreCase) ?? false) ||
        (UserData.Role?.Contains("Power", StringComparison.OrdinalIgnoreCase) ?? false);

    public void SetSession(
        string tenantCode,
        string authToken,
        string tenantName = "",
        int userCode = 0,
        int? branchCode = null,
        int? counterCode = null,
        UserDetailsModel? userData = null,
        IList<User_Rights>? userRights = null)
    {
        if (IsAuthenticated && AuthToken == authToken)
        {
            if (!string.IsNullOrEmpty(tenantCode)) TenantCode = tenantCode;
            if (!string.IsNullOrEmpty(tenantName)) TenantName = tenantName;
            if (userCode != 0) UserCode = userCode;
            if (branchCode != null) BranchCode = branchCode;
            if (counterCode != null) CounterCode = counterCode;
            if (userData != null) UserData = userData;
            if (userRights != null) UserRightsList = userRights;
            OnSessionChanged?.Invoke();
            return;
        }

        TenantCode = tenantCode ?? string.Empty;
        AuthToken = authToken ?? string.Empty;
        TenantName = tenantName ?? string.Empty;
        UserCode = userCode;
        BranchCode = branchCode;
        CounterCode = counterCode;
        UserData = userData;
        if (userRights != null)
        {
            UserRightsList = userRights;
        }
        OnSessionChanged?.Invoke();
    }

    public void Clear()
    {
        TenantCode = string.Empty;
        AuthToken = string.Empty;
        TenantName = string.Empty;
        BranchCode = null;
        CounterCode = null;
        UserData = null;
        UserRightsList = new List<User_Rights>();
        OnSessionChanged?.Invoke();
    }

    public bool HasActionRight(string department, string moduleName, string actionType)
    {
        // 1. Instant Admin / Power User / System User access
        if (IsAdmin)
        {
            return true;
        }

        // 2. If rights list is not yet loaded or empty, fallback to true for authenticated users
        if (UserRightsList == null || !UserRightsList.Any())
        {
            return true;
        }

        // 3. Exact Match (Department + ModuleName)
        var right = UserRightsList.FirstOrDefault(r => 
            string.Equals(r.Department?.Trim(), department?.Trim(), StringComparison.OrdinalIgnoreCase) && 
            string.Equals(r.ModuleName?.Trim(), moduleName?.Trim(), StringComparison.OrdinalIgnoreCase));

        // 4. ModuleName match across any Department
        right ??= UserRightsList.FirstOrDefault(r => 
            string.Equals(r.ModuleName?.Trim(), moduleName?.Trim(), StringComparison.OrdinalIgnoreCase));

        // 5. Department match with "General" or "*"
        right ??= UserRightsList.FirstOrDefault(r => 
            string.Equals(r.Department?.Trim(), department?.Trim(), StringComparison.OrdinalIgnoreCase) &&
            (string.Equals(r.ModuleName?.Trim(), "General", StringComparison.OrdinalIgnoreCase) || string.Equals(r.ModuleName?.Trim(), "*", StringComparison.OrdinalIgnoreCase)));

        // 6. Global General right
        right ??= UserRightsList.FirstOrDefault(r => 
            string.Equals(r.Department?.Trim(), "General", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.ModuleName?.Trim(), "General", StringComparison.OrdinalIgnoreCase));

        if (right == null)
        {
            return false;
        }

        return actionType switch
        {
            "Create" => right.ToAdd,
            "Edit" => right.ToEdit,
            "Delete" => right.ToDelete,
            "View" => right.ToView,
            "Export" => right.ToExport,
            _ => true
        };
    }
}
