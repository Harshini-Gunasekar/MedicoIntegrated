using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using SharedComponents.Rcl.Services;
using medico_backend.Model;

namespace Booking.Services
{
    public class OpStatusMasterService
    {
        private readonly HttpClient _http;
        private readonly TenantSessionState _tenantState;

        public OpStatusMasterService(HttpClient http, TenantSessionState tenantState)
        {
            _http = http;
            _tenantState = tenantState;
        }

        private void EnsureTenantHeader(HttpRequestMessage request)
        {
            if (!string.IsNullOrWhiteSpace(_tenantState?.TenantCode) &&
                !request.Headers.Contains("tenant_code"))
            {
                request.Headers.Add("tenant_code", _tenantState.TenantCode);
            }
        }

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
                "REPORT" => "REPORT",
                "BOOKED" => "BOOKED",
                "ARRIVED" => "ARRIVED",
                _ => status.Trim().ToUpperInvariant()
            };
        }

        public async Task<List<OpStatusMasterModel>> GetAllStatusesAsync(bool activeOnly = false)
        {
            try
            {
                var url = activeOnly ? "api/OpStatusMaster/all?activeOnly=true" : "api/OpStatusMaster/all";
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                EnsureTenantHeader(request);

                var response = await _http.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException(
                        $"Loading OP statuses failed with HTTP {(int)response.StatusCode} ({response.StatusCode}).");
                }

                var json = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                using var document = JsonDocument.Parse(json);
                if (document.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var legacyStatuses = JsonSerializer.Deserialize<List<OpStatusMasterModel>>(json, options);
                    if (legacyStatuses == null)
                    {
                        throw new JsonException("The OP status API returned an invalid list.");
                    }

                    foreach (var status in legacyStatuses)
                    {
                        if (string.IsNullOrWhiteSpace(status.status_key))
                        {
                            status.status_key = NormalizeStatusCode(status.status_name);
                        }
                        if (string.IsNullOrWhiteSpace(status.display_status_name))
                        {
                            status.display_status_name = status.status_name;
                        }
                    }
                    return legacyStatuses;
                }

                var envelope = JsonSerializer.Deserialize<OpStatusApiResponse<List<OpStatusResponse>>>(json, options);
                if (envelope?.data != null)
                {
                    return envelope.data.Select((status, index) =>
                    {
                        var statusName = !string.IsNullOrWhiteSpace(status.default_status_name)
                            ? status.default_status_name
                            : (status.status_name ?? status.status_code);
                        var displayName = string.IsNullOrWhiteSpace(status.display_status_name)
                            ? statusName
                            : status.display_status_name;

                        return new OpStatusMasterModel
                        {
                            op_status_id = status.op_status_id ?? index + 1,
                            status_key = !string.IsNullOrWhiteSpace(status.status_key)
                                ? status.status_key
                                : NormalizeStatusCode(status.status_code),
                            status_name = statusName,
                            default_status_name = statusName,
                            display_status_name = displayName,
                            is_customized = status.is_customized ??
                                !string.Equals(displayName, statusName, StringComparison.OrdinalIgnoreCase),
                            is_fixed = status.is_fixed ?? true,
                            is_active = status.is_active ?? true,
                            sort_order = status.sort_order ?? index + 1,
                            tenant_code = status.tenant_code,
                            usercode = status.usercode,
                            created_at = status.created_at ?? default,
                            updated_at = status.updated_at ?? default
                        };
                    }).ToList();
                }

                throw new JsonException("The OP status API returned an unexpected response.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpStatusMasterService] Error fetching OP statuses: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveDisplayStatusAsync(string statusCode, string displayStatusName, int? usercode)
        {
            var code = NormalizeStatusCode(statusCode);

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
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpStatusMasterService] Error saving OP status display name: {ex.Message}");
                return false;
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
