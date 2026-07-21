using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using SharedComponents.Rcl.Services;

namespace Booking.Services
{
    public class TenantHeaderHandler : DelegatingHandler
    {
        private readonly TenantSessionState _session;
        private readonly ProtectedSessionStorage _sessionStorage;

        public TenantHeaderHandler(TenantSessionState session, ProtectedSessionStorage sessionStorage)
        {
            _session = session;
            _sessionStorage = sessionStorage;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var requestPath = request.RequestUri?.AbsolutePath ?? "";
            bool isAnonymousRegister = request.Headers.Contains("X-Anonymous-Register");
            if (isAnonymousRegister)
            {
                request.Headers.Remove("X-Anonymous-Register");
            }

            bool isAnonymous = requestPath.Contains("/login", StringComparison.OrdinalIgnoreCase) || 
                               requestPath.Contains("/forgot-password", StringComparison.OrdinalIgnoreCase) ||
                               requestPath.Contains("/Tenant/login", StringComparison.OrdinalIgnoreCase) ||
                               isAnonymousRegister;

            bool isPrerendering = false;
            if (string.IsNullOrEmpty(_session.TenantCode) && !isAnonymous)
            {
                try
                {
                    var tenantCodeResult = await _sessionStorage.GetAsync<string>("tenant_code");
                    var tenantCode = tenantCodeResult.Success ? tenantCodeResult.Value ?? "" : "";

                    var tokenResult = await _sessionStorage.GetAsync<string>("authToken");
                    var authToken = tokenResult.Success ? tokenResult.Value ?? "" : "";

                    var tenantNameResult = await _sessionStorage.GetAsync<string>("tenant_name");
                    var tenantName = tenantNameResult.Success ? tenantNameResult.Value ?? "" : "";

                    if (!string.IsNullOrEmpty(tenantCode) && !string.IsNullOrEmpty(authToken))
                    {
                        _session.SetSession(tenantCode, authToken, tenantName);
                    }
                }
                catch (Exception)
                {
                    isPrerendering = true;
                }
            }

            if (!string.IsNullOrEmpty(_session.TenantCode) && !isAnonymousRegister)
            {
                if (request.Headers.Contains("tenant_code"))
                    request.Headers.Remove("tenant_code");
                request.Headers.Add("tenant_code", _session.TenantCode);
            }
            else if (!isAnonymous && !isPrerendering)
            {
                throw new System.InvalidOperationException("Tenant context is not initialized. Please log in again.");
            }

            if (isAnonymousRegister)
            {
                request.Headers.Remove("Authorization");
            }
            else if (!string.IsNullOrEmpty(_session.AuthToken) && !request.Headers.Contains("Authorization"))
            {
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.AuthToken);
            }

            var method = request.Method.Method;
            var url = request.RequestUri?.ToString();
            Console.WriteLine($"[HTTP Request] Outgoing: {method} {url}");
            foreach (var h in request.Headers)
            {
                Console.WriteLine($"  {h.Key}: {string.Join(", ", h.Value)}");
            }

            try
            {
                var response = await base.SendAsync(request, cancellationToken);
                var statusCode = (int)response.StatusCode;
                Console.WriteLine($"[HTTP Response] Completed: {method} {url} -> {statusCode} ({response.StatusCode})");
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HTTP Error] Failed: {method} {url} -> {ex.Message}");
                throw;
            }
        }
    }
}
