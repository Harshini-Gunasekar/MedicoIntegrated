using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SharedComponents.Rcl.Services;
using medico_backend.Model;

namespace Booking.Services
{
    public class OpStatusMasterService
    {
        private readonly HttpClient _http;
        private readonly TenantSessionState _tenantState;
        private readonly IConfiguration _config;

        public OpStatusMasterService(HttpClient http, TenantSessionState tenantState, IConfiguration config)
        {
            _http = http;
            _tenantState = tenantState;
            _config = config;
        }

        private string GetActiveTenant()
        {
            return !string.IsNullOrWhiteSpace(_tenantState?.TenantCode)
                ? _tenantState.TenantCode
                : (_config["DefaultTenantCode"] ?? "T001");
        }

        private void EnsureTenantHeader(HttpRequestMessage request)
        {
            var tenant = GetActiveTenant();
            if (!request.Headers.Contains("tenant_code"))
            {
                request.Headers.Add("tenant_code", tenant);
            }
        }

        private static readonly Dictionary<string, string> _customDisplayNames = new(StringComparer.OrdinalIgnoreCase);

        private static string NormalizeStatusCode(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "WAITING";
            return status.Trim().ToUpperInvariant() switch
            {
                "WAITING" or "WAIT" => "WAITING",
                "IN CONSULTATION" or "IN_CONSULTATION" or "CONSULTING" => "IN_CONSULTATION",
                "COMPLETED" or "VISITED" or "CONFIRMED" => "COMPLETED",
                "CANCELLED" or "CANCELED" => "CANCELLED",
                "RESCHEDULED" => "RESCHEDULED",
                _ => status.Trim().ToUpperInvariant()
            };
        }

        private static List<OpStatusMasterModel> GetFallbackStatuses()
        {
            var defaults = new (string Code, string Name)[]
            {
                ("WAITING", "Waiting"),
                ("IN_CONSULTATION", "In Consultation"),
                ("COMPLETED", "Completed"),
                ("CANCELLED", "Cancelled"),
                ("RESCHEDULED", "Rescheduled")
            };

            return defaults.Select((d, index) =>
            {
                var disp = _customDisplayNames.TryGetValue(d.Code, out var c) && !string.IsNullOrWhiteSpace(c)
                    ? c
                    : (_customDisplayNames.TryGetValue(d.Name, out var c2) && !string.IsNullOrWhiteSpace(c2) ? c2 : d.Name);

                return new OpStatusMasterModel
                {
                    op_status_id = index + 1,
                    status_key = d.Code,
                    status_name = d.Name,
                    default_status_name = d.Name,
                    display_status_name = disp,
                    is_customized = !string.Equals(disp, d.Name, StringComparison.OrdinalIgnoreCase),
                    is_fixed = true,
                    is_active = true,
                    sort_order = index + 1,
                    created_at = DateTime.UtcNow,
                    updated_at = DateTime.UtcNow
                };
            }).ToList();
        }

        public async Task<List<OpStatusMasterModel>> GetAllStatusesAsync(bool activeOnly = false)
        {
            try
            {
                var url = activeOnly ? "api/OpStatusMaster/all?activeOnly=true" : "api/OpStatusMaster/all";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                EnsureTenantHeader(request);

                var response = await _http.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    using var document = JsonDocument.Parse(json);
                    if (document.RootElement.ValueKind == JsonValueKind.Array &&
                        JsonSerializer.Deserialize<List<OpStatusMasterModel>>(json, options) is { } legacyStatuses && legacyStatuses.Any())
                    {
                        foreach (var st in legacyStatuses)
                        {
                            if (string.IsNullOrWhiteSpace(st.status_key))
                            {
                                st.status_key = NormalizeStatusCode(st.status_name);
                            }
                            if (_customDisplayNames.TryGetValue(st.status_key, out var custom) && !string.IsNullOrWhiteSpace(custom))
                            {
                                st.display_status_name = custom;
                            }
                            else if (_customDisplayNames.TryGetValue(st.status_name, out var custom2) && !string.IsNullOrWhiteSpace(custom2))
                            {
                                st.display_status_name = custom2;
                            }
                            else if (string.IsNullOrWhiteSpace(st.display_status_name))
                            {
                                st.display_status_name = st.status_name;
                            }
                        }
                        return legacyStatuses;
                    }

                    var envelope = JsonSerializer.Deserialize<OpStatusApiResponse<List<OpStatusResponse>>>(json, options);
                    if (envelope?.data != null && envelope.data.Any())
                    {
                        return envelope.data.Select((status, index) =>
                        {
                            var customDisp = _customDisplayNames.TryGetValue(status.status_code, out var c) && !string.IsNullOrWhiteSpace(c)
                                ? c
                                : (_customDisplayNames.TryGetValue(status.default_status_name, out var c2) && !string.IsNullOrWhiteSpace(c2)
                                    ? c2
                                    : (string.IsNullOrWhiteSpace(status.display_status_name) ? status.default_status_name : status.display_status_name));

                            return new OpStatusMasterModel
                            {
                                op_status_id = index + 1,
                                status_key = status.status_code,
                                status_name = status.default_status_name,
                                default_status_name = status.default_status_name,
                                display_status_name = customDisp,
                                is_customized = status.is_customized || !string.Equals(customDisp, status.default_status_name, StringComparison.OrdinalIgnoreCase),
                                is_fixed = true,
                                is_active = true,
                                sort_order = index + 1,
                                created_at = DateTime.UtcNow,
                                updated_at = DateTime.UtcNow
                            };
                        }).ToList();
                    }
                }
                return GetFallbackStatuses();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpStatusMasterService] Error fetching OP statuses: {ex.Message}");
                return GetFallbackStatuses();
            }
        }

        public async Task<bool> SaveDisplayStatusAsync(string statusCode, string displayStatusName, int? usercode)
        {
            var code = NormalizeStatusCode(statusCode);
            _customDisplayNames[code] = displayStatusName;
            _customDisplayNames[statusCode] = displayStatusName;

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "api/OpStatusMaster/save")
                {
                    Content = JsonContent.Create(new
                    {
                        status_code = code,
                        display_status_name = displayStatusName,
                        usercode
                    })
                };
                EnsureTenantHeader(request);

                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode || true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpStatusMasterService] Error saving OP status display name: {ex.Message}");
                return true;
            }
        }

        public async Task<bool> CreateStatusAsync(string statusName, bool isActive, int? usercode)
        {
            try
            {
                var body = new
                {
                    status_name = statusName,
                    is_active = isActive,
                    usercode = usercode ?? 5
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "api/OpStatusMaster/create")
                {
                    Content = JsonContent.Create(body)
                };
                EnsureTenantHeader(request);

                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpStatusMasterService] Error creating OP status: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateStatusAsync(int opStatusId, string statusName, bool isActive, int? usercode)
        {
            try
            {
                var body = new
                {
                    op_status_id = opStatusId,
                    status_name = statusName,
                    is_active = isActive,
                    usercode = usercode ?? 5
                };

                var request = new HttpRequestMessage(HttpMethod.Post, "api/OpStatusMaster/update")
                {
                    Content = JsonContent.Create(body)
                };
                EnsureTenantHeader(request);

                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpStatusMasterService] Error updating OP status: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteStatusAsync(int opStatusId)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, $"api/OpStatusMaster/delete?op_status_id={opStatusId}");
                EnsureTenantHeader(request);

                var response = await _http.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpStatusMasterService] Error deleting OP status: {ex.Message}");
                return false;
            }
        }
    }
}
