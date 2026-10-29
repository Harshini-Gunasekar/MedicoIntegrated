using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Booking.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Booking.Services
{
    public class HealthAppSettings
    {
        public string? BaseUrl { get; set; }
        public string? ApiBaseUrl { get; set; }
        public string? UniIdentityBaseUrl { get; set; }
        public string? RidoBaseUrl { get; set; }
        public string? UserRightUrl { get; set; }
        public string? LabCareUrl { get; set; }
        public string? BarcodeApiUrl { get; set; }
        public string? MedicoApiUrl { get; set; }
        public string App { get; set; } = "Medico Web";
    }

    public class HealthService
    {
        private readonly HealthAppSettings _settings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HealthService> _logger;

        public HealthService(
            HealthAppSettings? settings,
            IHttpClientFactory httpClientFactory,
            ILogger<HealthService> logger,
            IConfiguration? configuration = null)
        {
            _settings = settings ?? new HealthAppSettings();
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            // Fallback to configuration if settings properties are empty
            if (configuration != null)
            {
                if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
                    _settings.BaseUrl = configuration["ApiBaseUrl"] ?? configuration["BaseUrl"];

                if (string.IsNullOrWhiteSpace(_settings.ApiBaseUrl))
                    _settings.ApiBaseUrl = configuration["ApiBaseUrl"] ?? _settings.BaseUrl;

                if (string.IsNullOrWhiteSpace(_settings.UniIdentityBaseUrl))
                    _settings.UniIdentityBaseUrl = configuration["RidoUrl"] ?? configuration["UniIdentityBaseUrl"];

                if (string.IsNullOrWhiteSpace(_settings.RidoBaseUrl))
                    _settings.RidoBaseUrl = configuration["UserRightUrl"] ?? configuration["RidoBaseUrl"] ?? _settings.UniIdentityBaseUrl;

                if (string.IsNullOrWhiteSpace(_settings.UserRightUrl))
                    _settings.UserRightUrl = configuration["UserRightUrl"] ?? _settings.RidoBaseUrl;

                if (string.IsNullOrWhiteSpace(_settings.LabCareUrl))
                    _settings.LabCareUrl = configuration["LabCareUrl"];

                if (string.IsNullOrWhiteSpace(_settings.BarcodeApiUrl))
                    _settings.BarcodeApiUrl = configuration["BarcodeApiUrl"];

                if (string.IsNullOrWhiteSpace(_settings.MedicoApiUrl))
                    _settings.MedicoApiUrl = configuration["MedicoApiUrl"] ?? _settings.ApiBaseUrl;
            }
        }

        public async Task<HealthCheckResponse> CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            var response = new HealthCheckResponse
            {
                App = !string.IsNullOrWhiteSpace(_settings.App) ? _settings.App : "Medico Web",
                CheckedAtUtc = DateTime.UtcNow.ToString("o")
            };

            var checkTasks = new List<Task<ApiHealthDetail>>();

            // 1. Check Main Backend API (calls its /Health/Check or /api/Health/Check to verify DB)
            var mainBackendUrl = !string.IsNullOrWhiteSpace(_settings.ApiBaseUrl)
                ? _settings.ApiBaseUrl
                : _settings.BaseUrl;

            if (!string.IsNullOrWhiteSpace(mainBackendUrl))
            {
                checkTasks.Add(CheckBackendApiAsync("Medico API", mainBackendUrl, cancellationToken));
            }

            // 2. Check UniIdentity API
            var uniIdentityUrl = !string.IsNullOrWhiteSpace(_settings.UniIdentityBaseUrl)
                ? _settings.UniIdentityBaseUrl
                : _settings.RidoBaseUrl;
            if (!string.IsNullOrWhiteSpace(uniIdentityUrl))
            {
                checkTasks.Add(CheckPingableApiAsync("UniIdentity API", uniIdentityUrl, cancellationToken));
            }

            // 3. Check Rido API (if separate)
            var ridoUrl = !string.IsNullOrWhiteSpace(_settings.UserRightUrl)
                ? _settings.UserRightUrl
                : _settings.RidoBaseUrl;
            if (!string.IsNullOrWhiteSpace(ridoUrl) &&
                !string.Equals(ridoUrl, uniIdentityUrl, StringComparison.OrdinalIgnoreCase))
            {
                checkTasks.Add(CheckPingableApiAsync("Rido API", ridoUrl, cancellationToken));
            }

            // 4. Check Barcode Print / Report Server API
            if (!string.IsNullOrWhiteSpace(_settings.BarcodeApiUrl))
            {
                checkTasks.Add(CheckBarcodeApiAsync("Barcode API", _settings.BarcodeApiUrl, cancellationToken));
            }

            // 5. Check LabCare API
            if (!string.IsNullOrWhiteSpace(_settings.LabCareUrl))
            {
                checkTasks.Add(CheckPingableApiAsync("LabCare API", _settings.LabCareUrl, cancellationToken));
            }

            // 6. Check Medico API (if configured separately as MedicoApiUrl and different from mainBackendUrl)
            if (!string.IsNullOrWhiteSpace(_settings.MedicoApiUrl) &&
                !string.Equals(_settings.MedicoApiUrl, mainBackendUrl, StringComparison.OrdinalIgnoreCase))
            {
                checkTasks.Add(CheckPingableApiAsync("Medico API", _settings.MedicoApiUrl, cancellationToken));
            }

            var results = await Task.WhenAll(checkTasks);
            response.Apis = results.ToList();

            var unhealthyApis = response.Apis.Where(a => !string.Equals(a.Status, "healthy", StringComparison.OrdinalIgnoreCase)).ToList();

            if (unhealthyApis.Count > 0)
            {
                response.Status = "unhealthy";
                response.Message = string.Join("; ", unhealthyApis.Select(u => $"{u.Name}: {u.Message}"));
            }
            else
            {
                response.Status = "healthy";
                response.Message = "";
            }

            return response;
        }

        private async Task<ApiHealthDetail> CheckBackendApiAsync(string name, string baseUrl, CancellationToken ct)
        {
            var detail = new ApiHealthDetail
            {
                Name = name,
                Url = baseUrl
            };

            var sw = Stopwatch.StartNew();
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                var normalizedBase = baseUrl.TrimEnd('/');
                var checkUrl = normalizedBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase)
                    ? $"{normalizedBase}/Health/Check"
                    : $"{normalizedBase}/api/Health/Check";

                HttpResponseMessage res;
                try
                {
                    res = await client.GetAsync(checkUrl, cts.Token);
                    if (res.StatusCode == System.Net.HttpStatusCode.NotFound && !normalizedBase.EndsWith("/api", StringComparison.OrdinalIgnoreCase))
                    {
                        var fallbackUrl = $"{normalizedBase}/Health/Check";
                        var fallbackRes = await client.GetAsync(fallbackUrl, cts.Token);
                        if (fallbackRes.IsSuccessStatusCode || (int)fallbackRes.StatusCode < 500)
                        {
                            res = fallbackRes;
                        }
                    }
                }
                catch when (!cts.IsCancellationRequested)
                {
                    var fallbackUrl = $"{normalizedBase}/Health/Check";
                    res = await client.GetAsync(fallbackUrl, cts.Token);
                }

                sw.Stop();
                detail.ResponseTimeMs = sw.ElapsedMilliseconds;

                var body = await res.Content.ReadAsStringAsync(cts.Token);

                if (!string.IsNullOrWhiteSpace(body))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(body);
                        var root = doc.RootElement;
                        var status = root.TryGetProperty("status", out var sProp) ? sProp.GetString() : null;
                        var msg = root.TryGetProperty("message", out var mProp) ? mProp.GetString() : null;

                        if (string.Equals(status, "healthy", StringComparison.OrdinalIgnoreCase))
                        {
                            detail.Status = "healthy";
                            detail.Message = "";
                            return detail;
                        }

                        if (!string.IsNullOrEmpty(status))
                        {
                            detail.Status = "unhealthy";
                            detail.Message = !string.IsNullOrWhiteSpace(msg)
                                ? msg
                                : $"API returned status '{status}'";
                            return detail;
                        }
                    }
                    catch
                    {
                        // JSON parsing fallback
                    }
                }

                if (res.IsSuccessStatusCode)
                {
                    detail.Status = "healthy";
                    detail.Message = "";
                }
                else
                {
                    detail.Status = "unhealthy";
                    detail.Message = $"HTTP {(int)res.StatusCode} {res.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                detail.ResponseTimeMs = sw.ElapsedMilliseconds;
                detail.Status = "unhealthy";
                detail.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return detail;
        }

        private Task<ApiHealthDetail> CheckyourappApiAsync(string baseUrl, CancellationToken ct)
        {
            return CheckBackendApiAsync("Medico API", baseUrl, ct);
        }

        private async Task<ApiHealthDetail> CheckBarcodeApiAsync(string name, string url, CancellationToken ct)
        {
            var detail = new ApiHealthDetail
            {
                Name = name,
                Url = url
            };

            var sw = Stopwatch.StartNew();
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                var res = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                sw.Stop();
                detail.ResponseTimeMs = sw.ElapsedMilliseconds;

                if ((int)res.StatusCode < 500)
                {
                    detail.Status = "healthy";
                    detail.Message = "";
                }
                else
                {
                    detail.Status = "unhealthy";
                    detail.Message = $"Server error: HTTP {(int)res.StatusCode} {res.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                detail.ResponseTimeMs = sw.ElapsedMilliseconds;
                detail.Status = "unhealthy";
                detail.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return detail;
        }

        private async Task<ApiHealthDetail> CheckPingableApiAsync(string name, string url, CancellationToken ct)
        {
            var detail = new ApiHealthDetail
            {
                Name = name,
                Url = url
            };

            var sw = Stopwatch.StartNew();
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(TimeSpan.FromSeconds(5));

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(5);

                using var req = new HttpRequestMessage(HttpMethod.Get, url);
                var res = await client.SendAsync(req, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                sw.Stop();
                detail.ResponseTimeMs = sw.ElapsedMilliseconds;

                if ((int)res.StatusCode < 500)
                {
                    detail.Status = "healthy";
                    detail.Message = "";
                }
                else
                {
                    detail.Status = "unhealthy";
                    detail.Message = $"Server error: HTTP {(int)res.StatusCode} {res.ReasonPhrase}";
                }
            }
            catch (Exception ex)
            {
                sw.Stop();
                detail.ResponseTimeMs = sw.ElapsedMilliseconds;
                detail.Status = "unhealthy";
                detail.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return detail;
        }
    }
}
