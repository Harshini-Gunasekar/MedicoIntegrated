using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace TrackAdmin.Data
{
    public class TenantHeaderHandler : DelegatingHandler
    {
        private readonly TenantState _tenantState;
        private readonly ProtectedSessionStorage _sessionStorage;

        public TenantHeaderHandler(TenantState tenantState, ProtectedSessionStorage sessionStorage)
        {
            _tenantState = tenantState;
            _sessionStorage = sessionStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Add TenantID Header (try both common casings)
            if (!string.IsNullOrWhiteSpace(_tenantState.TenantID))
            {
                // Completely clear any existing headers to prevent "T_0011,T_0011" doubling
                request.Headers.Remove("TenantID");
                request.Headers.Remove("TenantId");
                request.Headers.Remove("tenant_id");
                request.Headers.Remove("TenantCode");
                request.Headers.Remove("tenant_code");
                request.Headers.Remove("Tenant-ID");

                request.Headers.TryAddWithoutValidation("TenantId", _tenantState.TenantID);
            }

            // Add JWT Bearer Token Header (BUT NOT for Login or Tenant auto-resolve calls)
            var url = request.RequestUri?.ToString().ToLower() ?? "";
            if (!url.Contains("login") && !url.Contains("getidbyhost"))
            {
                try
                {
                    var result = await _sessionStorage.GetAsync<UserSession>("UserSession");
                    if (result.Success && result.Value != null && !string.IsNullOrWhiteSpace(result.Value.Token))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", result.Value.Token);
                    }
                }
                catch { }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}