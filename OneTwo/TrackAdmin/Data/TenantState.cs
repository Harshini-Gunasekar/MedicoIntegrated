public class TenantState
{
    private string _tenantId = "";
    private string _logoUrl = "/favicon.png";
    private string _tenantName = "";

    public string TenantID 
    { 
        get => _tenantId; 
        set 
        { 
            if (_tenantId != value) 
            { 
                _tenantId = value; 
                NotifyStateChanged(); 
            } 
        } 
    }

    public string LogoUrl 
    { 
        get
        {
            if (string.IsNullOrEmpty(_logoUrl) || 
                _logoUrl == "/favicon.png" || 
                _logoUrl == "favicon.png")
            {
                return "/images/rido-logo-pro.png";
            }
            return _logoUrl;
        }
        set 
        { 
            if (_logoUrl != value) 
            { 
                _logoUrl = value; 
                NotifyStateChanged(); 
            } 
        } 
    }

    public string TenantName 
    { 
        get => _tenantName; 
        set 
        { 
            if (_tenantName != value) 
            { 
                _tenantName = value; 
                NotifyStateChanged(); 
            } 
        } 
    }
    
    public void SetLogo(string url)
    {
        LogoUrl = url;
    }

    public void SetTenantName(string name)
    {
        TenantName = name;
    }
    // Branch Context
    public int? SelectedBranchCode { get; set; }
    public string SelectedBranchName { get; set; } = "All Branches";

    // App Context
    public string SelectedApp { get; set; } = "Rido"; // Default
    public List<string> AccessibleApps { get; set; } = new();

    // Global Chat Request Context
    public string? ChatOpenRiderId { get; set; }
    public string? ChatOpenRiderName { get; set; }

    public void RequestOpenChat(string riderId, string riderName)
    {
        ChatOpenRiderId = riderId;
        ChatOpenRiderName = riderName;
        NotifyStateChanged();
    }

    public void ClearOpenChatRequest()
    {
        ChatOpenRiderId = null;
        ChatOpenRiderName = null;
    }

    public event Action OnStateChange;

    public void SetBranch(int code, string name)
    {
        SelectedBranchCode = code;
        SelectedBranchName = name;
        NotifyStateChanged();
    }

    public void SetApp(string app)
    {
        SelectedApp = app;
        NotifyStateChanged();
    }

    private void NotifyStateChanged() => OnStateChange?.Invoke();
}