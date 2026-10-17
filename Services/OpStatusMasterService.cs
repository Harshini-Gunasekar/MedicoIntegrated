using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
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
                    return await response.Content.ReadFromJsonAsync<List<OpStatusMasterModel>>() ?? new List<OpStatusMasterModel>();
                }
                return new List<OpStatusMasterModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OpStatusMasterService] Error fetching OP statuses: {ex.Message}");
                return new List<OpStatusMasterModel>();
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
