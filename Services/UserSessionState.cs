using Microsoft.Extensions.Configuration;
using SharedComponents.Rcl.Services;

namespace MedicoAi.Services
{
    public class UserSessionState
    {
        private readonly TenantSessionState _tenantState;
        
        public UserSessionState(IConfiguration configuration, TenantSessionState tenantState)
        {
            _tenantState = tenantState;
            Role = configuration["MedicoAi:Role"] ?? "Chief Medical Officer";
            ApiBaseUrl = configuration["MedicoAi:ApiBaseUrl"] ?? "http://medicoapi.iscansoft.com";
            AiApiBaseUrl = configuration["MedicoAi:AiApiBaseUrl"] ?? "https://ai.seyotechnologies.com";
            AiModel = configuration["MedicoAi:AiModel"] ?? "gemini-1.5-flash";
            GeminiApiKey = configuration["MedicoAi:GeminiApiKey"] ?? configuration["AI_GEMINI_API_KEY"] ?? string.Empty;
            ActiveProvider = string.IsNullOrWhiteSpace(GeminiApiKey) ? "Local Fallback" : (configuration["MedicoAi:ActiveProvider"] ?? "Gemini API");
        }

        public bool IsLoggedIn => _tenantState.IsAuthenticated;
        public string TenantCode => _tenantState.TenantCode;
        public string DoctorName => string.IsNullOrWhiteSpace(_tenantState.UserData?.UserName) ? "Dr. Santhosh" : _tenantState.UserData.UserName;
        public string Role { get; set; }
        public string ApiBaseUrl { get; set; }
        public string AiApiBaseUrl { get; set; }
        public string AiModel { get; set; }
        public string GeminiApiKey { get; set; }
        public string ActiveProvider { get; set; }

        public event Action? OnStateChanged
        {
            add => _tenantState.OnSessionChanged += value;
            remove => _tenantState.OnSessionChanged -= value;
        }

        public void Login(string username, string tenantCode)
        {
        }

        public void Logout()
        {
            _tenantState.Clear();
        }

        public void UpdateTenantCode(string newTenantCode)
        {
        }

        public void NotifyStateChanged() => _tenantState.SetSession(_tenantState.TenantCode, _tenantState.AuthToken, _tenantState.TenantName, _tenantState.UserCode, _tenantState.BranchCode, _tenantState.CounterCode, _tenantState.UserData, _tenantState.UserRightsList);
    }
}
