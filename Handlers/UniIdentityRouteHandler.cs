using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SharedComponents.Rcl.Services;

namespace Booking.Handlers
{
    public class UniIdentityRouteHandler : DelegatingHandler
    {
        private readonly string _baseUrl;
        private readonly string _uniIdentityBaseUrl;
        private readonly TenantSessionState _session;

        public UniIdentityRouteHandler(IConfiguration config, TenantSessionState session)
        {
            _baseUrl = config["ApiBaseUrl"] ?? "http://medicoapi.iscansoft.com";
            _uniIdentityBaseUrl = config["UserRightUrl"] ?? "https://ridoapi.iscansoft.com/api/";
            _session = session;
            
            // Ensure trailing slashes
            if (!_baseUrl.EndsWith("/")) _baseUrl += "/";
            if (!_uniIdentityBaseUrl.EndsWith("/")) _uniIdentityBaseUrl += "/";
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var originalUrl = request.RequestUri?.ToString() ?? "";
            
            // Match the specific endpoints called by UserRightsManagement
            if (originalUrl.Contains("/Tenant/GetTenantProducts", StringComparison.OrdinalIgnoreCase) ||
                originalUrl.Contains("/User/GetProductFeatures", StringComparison.OrdinalIgnoreCase) ||
                originalUrl.Contains("/User/allstaff", StringComparison.OrdinalIgnoreCase) ||
                originalUrl.Contains("/User/GetRoleTemplates", StringComparison.OrdinalIgnoreCase) ||
                originalUrl.Contains("/User/GetPermissions", StringComparison.OrdinalIgnoreCase) ||
                originalUrl.Contains("/User/SavePermissions", StringComparison.OrdinalIgnoreCase))
            {
                if (originalUrl.StartsWith(_baseUrl, StringComparison.OrdinalIgnoreCase))
                {
                    var relativePath = originalUrl.Substring(_baseUrl.Length);
                    var newUrl = _uniIdentityBaseUrl + relativePath;

                    // Read tenant code from the header that TenantHeaderHandler already injected.
                    // We do NOT use _session.TenantCode here because DelegatingHandlers are resolved
                    // from the IHttpClientFactory's internal scope, which may give a different
                    // TenantSessionState instance than the one the component is using.
                    string tenantCode = "";
                    if (request.Headers.TryGetValues("tenant_code", out var tcVals))
                        tenantCode = tcVals.FirstOrDefault() ?? "";
                    if (string.IsNullOrEmpty(tenantCode) && !string.IsNullOrEmpty(_session.TenantCode))
                        tenantCode = _session.TenantCode;

                    if (!string.IsNullOrEmpty(tenantCode))
                    {
                        // 1. Add TenantId to query string (ridoapi also reads it from query)
                        var separator = newUrl.Contains("?") ? "&" : "?";
                        if (!newUrl.Contains("TenantId=", StringComparison.OrdinalIgnoreCase))
                        {
                            newUrl += $"{separator}TenantId={tenantCode}";
                            separator = "&";
                        }
                        if (!newUrl.Contains("tenantCode=", StringComparison.OrdinalIgnoreCase) &&
                            !newUrl.Contains("tenant_code=", StringComparison.OrdinalIgnoreCase))
                        {
                            newUrl += $"{separator}tenantCode={tenantCode}";
                        }

                        request.RequestUri = new Uri(newUrl);

                        // 2. Add TenantId as a request HEADER — ridoapi requires "TenantId header is missing"
                        if (!request.Headers.Contains("TenantId"))
                            request.Headers.Add("TenantId", tenantCode);
                        if (!request.Headers.Contains("tenantCode"))
                            request.Headers.Add("tenantCode", tenantCode);
                        if (!request.Headers.Contains("tenant_id"))
                            request.Headers.Add("tenant_id", tenantCode);
                    }
                    else
                    {
                        request.RequestUri = new Uri(newUrl);
                    }
                }
            }

            HttpResponseMessage response = null;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
                
                // Log the request and response details
                var logStr = $"[{DateTime.Now}] Intercepted:\n" +
                             $"  Method: {request.Method}\n" +
                             $"  Original URL: {originalUrl}\n" +
                             $"  Rewritten URL: {request.RequestUri}\n" +
                             $"  Headers:\n" +
                             string.Join("\n", request.Headers.Select(h => $"    {h.Key}: {string.Join(", ", h.Value)}")) + "\n" +
                             $"  Response: {response.StatusCode}\n" +
                             $"  Response Body: {await (response.Content?.ReadAsStringAsync() ?? Task.FromResult(""))}\n\n";
                System.IO.File.AppendAllText(@"d:\Iscan\Medico\route_debug.txt", logStr);
            }
            catch (Exception ex)
            {
                var logStr = $"[{DateTime.Now}] Failed:\n" +
                             $"  Method: {request.Method}\n" +
                             $"  Original URL: {originalUrl}\n" +
                             $"  Rewritten URL: {request.RequestUri}\n" +
                             $"  Exception: {ex}\n\n";
                System.IO.File.AppendAllText(@"d:\Iscan\Medico\route_debug.txt", logStr);
                throw;
            }
            return response;
        }
    }
}
