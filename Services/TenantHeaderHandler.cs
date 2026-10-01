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
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

        public TenantHeaderHandler(TenantSessionState session, ProtectedSessionStorage sessionStorage, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _session = session;
            _sessionStorage = sessionStorage;
            _config = config;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.RequestUri != null)
            {
                var uriStr = request.RequestUri.ToString();
                if (uriStr.Contains("/api/api/", StringComparison.OrdinalIgnoreCase))
                {
                    uriStr = uriStr.Replace("/api/api/", "/api/", StringComparison.OrdinalIgnoreCase);
                    request.RequestUri = new Uri(uriStr);
                }
                else
                {
                    var configuredApiBaseUrl = _config["ApiBaseUrl"];
                    string? configuredHost = null;
                    if (!string.IsNullOrEmpty(configuredApiBaseUrl) && Uri.TryCreate(configuredApiBaseUrl, UriKind.Absolute, out var baseUri))
                    {
                        configuredHost = baseUri.Host;
                    }

                    bool isApiHost = (configuredHost != null && request.RequestUri.Host.Equals(configuredHost, StringComparison.OrdinalIgnoreCase)) ||
                                     request.RequestUri.Host.Equals("medicoapi.iscansoft.com", StringComparison.OrdinalIgnoreCase) ||
                                     request.RequestUri.Host.Equals("medicoapitest.reachbs.com", StringComparison.OrdinalIgnoreCase) ||
                                     request.RequestUri.Host.Equals("medicotestapi.reachbs.com", StringComparison.OrdinalIgnoreCase) ||
                                     request.RequestUri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                                     request.RequestUri.Host.Equals("127.0.0.1", StringComparison.OrdinalIgnoreCase);

                    if (isApiHost && !request.RequestUri.AbsolutePath.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
                    {
                        var builder = new UriBuilder(request.RequestUri);
                        builder.Path = "/api" + (builder.Path.StartsWith("/") ? builder.Path : "/" + builder.Path);
                        request.RequestUri = builder.Uri;
                    }
                }
            }

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

            string effectiveTenant = _session.TenantCode;

            if (string.IsNullOrEmpty(effectiveTenant))
            {
                if (request.Headers.TryGetValues("tenantcode", out var tcVals) && tcVals.Any() && !string.IsNullOrWhiteSpace(tcVals.First()))
                {
                    effectiveTenant = tcVals.First();
                }
                else if (request.Headers.TryGetValues("tenant_code", out var tcVals2) && tcVals2.Any() && !string.IsNullOrWhiteSpace(tcVals2.First()))
                {
                    effectiveTenant = tcVals2.First();
                }
                else if (request.Headers.TryGetValues("tenant-code", out var tcVals3) && tcVals3.Any() && !string.IsNullOrWhiteSpace(tcVals3.First()))
                {
                    effectiveTenant = tcVals3.First();
                }
            }

            if (string.IsNullOrEmpty(effectiveTenant) && !isAnonymous)
            {
                try
                {
                    var tenantCodeResult = await _sessionStorage.GetAsync<string>("tenant_code");
                    var tenantCode = tenantCodeResult.Success ? tenantCodeResult.Value ?? "" : "";

                    var tokenResult = await _sessionStorage.GetAsync<string>("authToken");
                    var authToken = tokenResult.Success ? tokenResult.Value ?? "" : "";

                    var tenantNameResult = await _sessionStorage.GetAsync<string>("tenant_name");
                    var tenantName = tenantNameResult.Success ? tenantNameResult.Value ?? "" : "";

                    if (!string.IsNullOrEmpty(tenantCode))
                    {
                        effectiveTenant = tenantCode;
                        if (!string.IsNullOrEmpty(authToken))
                        {
                            _session.SetSession(tenantCode, authToken, tenantName);
                        }
                    }
                }
                catch (Exception)
                {
                    // Session storage not accessible during prerendering or background invocation
                }
            }

            if (string.IsNullOrEmpty(effectiveTenant) && !isAnonymous)
            {
                effectiveTenant = "0010"; // standard default tenant code
            }

            if (!string.IsNullOrEmpty(effectiveTenant) && !isAnonymousRegister)
            {
                if (request.Headers.Contains("tenant_code"))
                    request.Headers.Remove("tenant_code");
                request.Headers.Add("tenant_code", effectiveTenant);

                if (request.Headers.Contains("tenantcode"))
                    request.Headers.Remove("tenantcode");
                request.Headers.Add("tenantcode", effectiveTenant);

                if (request.Headers.Contains("tenant-code"))
                    request.Headers.Remove("tenant-code");
                request.Headers.Add("tenant-code", effectiveTenant);
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
