using medico_backend.Model;
using SharedComponents.Rcl.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Booking.Services
{
    public class LabResultService
    {
        private readonly HttpClient _client;
        private readonly TenantSessionState _tenantState;

        public LabResultService(HttpClient client, TenantSessionState tenantState)
        {
            _client = client;
            _tenantState = tenantState;
        }

        private HttpClient CreateClient()
        {
            var client = _client;
            if (!string.IsNullOrEmpty(_tenantState.AuthToken) && !client.DefaultRequestHeaders.Contains("Authorization"))
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tenantState.AuthToken);
            }
            if (!string.IsNullOrEmpty(_tenantState.TenantCode) && !client.DefaultRequestHeaders.Contains("tenant_code"))
            {
                client.DefaultRequestHeaders.Add("tenant_code", _tenantState.TenantCode);
            }
            return client;
        }

        public async Task<List<ViewResultSearch>> ViewResultSearchAsync(string fromDate, string toDate)
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync($"api/labresult/viewresultsearch?fromdate={fromDate}&todate={toDate}");
                if (response.IsSuccessStatusCode)
                {
                    var list = await response.Content.ReadFromJsonAsync<List<ViewResultSearch>>() ?? new();
                    try
                    {
                        var testTask = client.GetFromJsonAsync<List<Booking.Models.TestMasterModel>>("api/Test/get");
                        var groupTask = client.GetFromJsonAsync<List<Booking.Models.GroupMasterModel>>("api/GroupMaster/get");
                        await Task.WhenAll(testTask, groupTask);

                        var tests = await testTask;
                        var groups = await groupTask;

                        var lockedTcodes = tests?.Where(t => t.lockresult).Select(t => (long)t.tcode).ToHashSet() ?? new();
                        var treatmentGcodes = groups?.Where(g => g.istreatment == true).Select(g => (long)g.gcode).ToHashSet() ?? new();

                        return list.Where(item => {
                            if (!string.IsNullOrWhiteSpace(item.tcode))
                            {
                                var clean = item.tcode.Replace("T-", "").Trim();
                                if (long.TryParse(clean, out long tc) && lockedTcodes.Contains(tc))
                                    return false;
                            }
                            if (item.gcode.HasValue && treatmentGcodes.Contains((long)item.gcode.Value))
                                return false;
                            return true;
                        }).ToList();
                    }
                    catch
                    {
                        return list;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LabResultService] Search error: {ex.Message}");
            }
            return new();
        }

        public async Task<ResultEntryModel?> LoadTestResultEntryAsync(string requestGuid)
        {
            try
            {
                var client = CreateClient();
                var response = await client.GetAsync($"api/LabResult/LoadResultEntry?requestguid={requestGuid}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ResultEntryModel>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LabResultService] Load error: {ex.Message}");
            }
            return null;
        }

        public async Task<bool> SaveResultEntryAsync(LabResultSaveRequest request)
        {
            try
            {
                if (request.lab_result_master != null && string.IsNullOrEmpty(request.lab_result_master.tenant_code))
                {
                    request.lab_result_master.tenant_code = _tenantState.TenantCode;
                }
                if (request.lab_result_details != null)
                {
                    foreach (var detail in request.lab_result_details)
                    {
                        if (string.IsNullOrEmpty(detail.tenant_code))
                        {
                            detail.tenant_code = _tenantState.TenantCode;
                        }
                    }
                }
                var client = CreateClient();
                var response = await client.PostAsJsonAsync("api/labresult/saveresultentry", request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LabResultService] Save error: {ex.Message}");
                return false;
            }
        }
    }
}
