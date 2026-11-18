using LabCare.Models;
using System.Collections.Generic;
using SharedComponents.Rcl.Models;

namespace LabCare.Services;

/// <summary>
/// Scoped state container that holds the active tenant's session data.
/// Populated by MainLayout when the session is loaded from ProtectedSessionStorage.
/// Consumed by TenantHeaderHandler to inject request headers automatically.
/// </summary>
public class TenantSessionState
{
    public string TenantCode { get; private set; } = string.Empty;
    public string AuthToken { get; private set; } = string.Empty;
    public string TenantName { get; private set; } = string.Empty;
    public int UserCode { get; private set; } = 0;
    public int? BranchCode { get; private set; }
    public int? CounterCode { get; private set; }
    
    public LabCare.Models.UserDetailsModel? UserData { get; private set; }
    public IList<User_Rights> UserRightsList { get; private set; } = new List<User_Rights>();

    public bool IsAuthenticated => !string.IsNullOrEmpty(AuthToken);

    public event Action? OnSessionChanged;

    public void SetSession(string tenantCode, string authToken, string tenantName = "", int userCode = 0, int? branchCode = null, int? counterCode = null, LabCare.Models.UserDetailsModel? userData = null, IList<User_Rights>? userRights = null)
    {
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
}
