namespace LabCare.Services;

/// <summary>
/// Defines every navigable module in the system.
/// Used as the key for permission lookups — match these
/// strings in the RolePermissions table's 'module_key' column.
/// </summary>
public static class AppModules
{
    // ── Core operational (always visible to Tenant Owner) ──────────────────
    public const string Billing         = "billing";
    public const string NewBilling      = "new-billing";
    public const string CustomerMaster  = "customer-master";
    public const string TestMaster      = "test-master";
    public const string ProMaster       = "pro-master";
    public const string ReferralDoctor  = "referral";
    public const string Collected       = "collected";

    // ── Rate & Financial ───────────────────────────────────────────────────
    public const string Rate            = "rate";
    public const string SpecialRate     = "special-rate";
    public const string ICMaster        = "ic-master";
    public const string FeeType         = "feetype";
    public const string PayMode         = "paymode";
    public const string ReimbursementCo = "reimbursement";

    // ── Admin / Master ─────────────────────────────────────────────────────
    public const string UserMaster      = "user-master";
    public const string BranchMaster    = "branch";
    public const string Counter         = "counter";
    public const string BillNo          = "billno";

    // ── Lab Science ────────────────────────────────────────────────────────
    public const string SampleMaster    = "sample";
    public const string ContainerMaster = "container";
    public const string UnitMaster      = "unit";
    public const string Qualification   = "qualification";

    // ── Geography ──────────────────────────────────────────────────────────
    public const string CountryMaster   = "country";
    public const string StateMaster     = "state";
    public const string CityMaster      = "city";
    public const string AreaMaster      = "area";

    /// <summary>
    /// Modules that are ONLY accessible to the Tenant Owner (role 0)
    /// or to users with explicit permission granted by the Tenant.
    /// Sub-users never see these unless unlocked.
    /// </summary>
    public static readonly HashSet<string> ProtectedModules = new(StringComparer.OrdinalIgnoreCase)
    {
        Billing, NewBilling, CustomerMaster, Collected,
        UserMaster, BranchMaster, Counter, BillNo,
        ICMaster, FeeType, PayMode, ReimbursementCo,
        SpecialRate, Rate
    };
}

/// <summary>
/// Scoped service: holds the fully-resolved user session including
/// role, permissions, and audit identity after login.
/// Populated once at login or NavMenu load; never call the API again.
/// </summary>
public class UserSessionService
{
    // ── Identity ───────────────────────────────────────────────────────────
    public int    UserCode   { get; private set; }
    public string UserName   { get; private set; } = string.Empty;
    public string ShortName  { get; private set; } = string.Empty;
    public string TenantCode { get; private set; } = string.Empty;
    public string TenantName { get; private set; } = string.Empty;
    public string AuthToken  { get; private set; } = string.Empty;

    // ── Role ───────────────────────────────────────────────────────────────
    /// <summary>
    /// 0 = Tenant Owner (Super-Admin, cannot be deleted/edited by others)
    /// 1 = Admin
    /// 2 = Staff
    /// 3 = Lab Tech
    /// 4 = Receptionist
    /// </summary>
    public int    RoleId     { get; private set; }
    public string RoleLabel  { get; private set; } = string.Empty;

    /// <summary>True when this user is the Tenant Owner (role 0). They bypass all module locks.</summary>
    public bool IsTenantOwner => RoleId == 0;

    /// <summary>True if a session is active.</summary>
    public bool IsAuthenticated => !string.IsNullOrEmpty(AuthToken);

    // ── Permissions: set of allowed module keys ────────────────────────────
    private HashSet<string> _allowedModules = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Load Tenant Owner session (comes from Tenant/login).
    /// Tenant Owner gets access to EVERY module automatically.
    /// </summary>
    public void SetTenantOwnerSession(
        string tenantCode, string tenantName,
        string authToken,
        string displayName = "")
    {
        TenantCode = tenantCode ?? string.Empty;
        TenantName = tenantName ?? string.Empty;
        AuthToken  = authToken  ?? string.Empty;
        UserName   = string.IsNullOrEmpty(displayName) ? tenantName ?? "Owner" : displayName;
        ShortName  = tenantCode ?? string.Empty;
        UserCode   = 0;
        RoleId     = 0;
        RoleLabel  = "Tenant Owner";

        // Unlock everything — Tenant Owner sees the full menu
        _allowedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            AppModules.Billing, AppModules.NewBilling, AppModules.CustomerMaster,
            AppModules.TestMaster, AppModules.ProMaster, AppModules.ReferralDoctor,
            AppModules.Collected, AppModules.Rate, AppModules.SpecialRate,
            AppModules.ICMaster, AppModules.FeeType, AppModules.PayMode,
            AppModules.ReimbursementCo, AppModules.UserMaster, AppModules.BranchMaster,
            AppModules.Counter, AppModules.BillNo, AppModules.SampleMaster,
            AppModules.ContainerMaster, AppModules.UnitMaster, AppModules.Qualification,
            AppModules.CountryMaster, AppModules.StateMaster, AppModules.CityMaster,
            AppModules.AreaMaster
        };
    }

    /// <summary>
    /// Load Sub-User (Staff) session (comes from User/login).
    /// Module access is restricted to what the Tenant has granted.
    /// </summary>
    public void SetStaffSession(
        int    userCode,
        string userName,
        string shortName,
        string tenantCode,
        string tenantName,
        string authToken,
        int    roleId,
        IEnumerable<string>? grantedModules = null)
    {
        UserCode   = userCode;
        UserName   = userName   ?? string.Empty;
        ShortName  = shortName  ?? string.Empty;
        TenantCode = tenantCode ?? string.Empty;
        TenantName = tenantName ?? string.Empty;
        AuthToken  = authToken  ?? string.Empty;
        RoleId     = roleId;
        RoleLabel  = GetDefaultRoleLabel(roleId);

        // Start with no protected modules; only add what Tenant unlocked
        _allowedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (grantedModules != null)
        {
            foreach (var m in grantedModules)
                _allowedModules.Add(m);
        }

        // Non-protected modules are always visible to staff
        _allowedModules.Add(AppModules.ReferralDoctor);
        _allowedModules.Add(AppModules.TestMaster);     // read-only context
        _allowedModules.Add(AppModules.SampleMaster);
        _allowedModules.Add(AppModules.ContainerMaster);
        _allowedModules.Add(AppModules.UnitMaster);
        _allowedModules.Add(AppModules.Qualification);
        _allowedModules.Add(AppModules.CountryMaster);
        _allowedModules.Add(AppModules.StateMaster);
        _allowedModules.Add(AppModules.CityMaster);
        _allowedModules.Add(AppModules.AreaMaster);
        _allowedModules.Add(AppModules.ProMaster);
    }

    /// <summary>Returns true if the current user can access a given module key.</summary>
    public bool CanAccess(string moduleKey)
    {
        if (IsTenantOwner) return true;              // Owner bypasses all locks
        return _allowedModules.Contains(moduleKey);
    }

    public void Clear()
    {
        UserCode   = 0;
        UserName   = string.Empty;
        ShortName  = string.Empty;
        TenantCode = string.Empty;
        TenantName = string.Empty;
        AuthToken  = string.Empty;
        RoleId     = -1;
        RoleLabel  = string.Empty;
        _allowedModules.Clear();
    }

    private static string GetDefaultRoleLabel(int roleId) => roleId switch
    {
        0 => "Tenant Owner",
        1 => "Admin",
        2 => "Staff",
        3 => "Lab Tech",
        4 => "Receptionist",
        _ => "User"
    };
}
