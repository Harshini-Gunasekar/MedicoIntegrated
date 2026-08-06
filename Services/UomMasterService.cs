using System.Net.Http.Json;
using Booking.Models;

namespace Booking.Services
{
    public class UomMasterService
    {
        private readonly HttpClient _http;
        private readonly IHttpClientFactory _clientFactory;
        private readonly SharedComponents.Rcl.Services.TenantSessionState _tenantState;

        public UomMasterService(HttpClient http, IHttpClientFactory clientFactory, SharedComponents.Rcl.Services.TenantSessionState tenantState)
        {
            _http = http;
            _clientFactory = clientFactory;
            _tenantState = tenantState;
        }

        public async Task<List<UomMasterModel>> GetUomMastersAsync()
        {
            try
            {
                var tc = _tenantState?.TenantCode;

                // Try fetching using relative endpoints on _http
                string[] endpoints = new[] { "api/ItemMaster/getalluom", "ItemMaster/getalluom" };
                
                foreach (var endpoint in endpoints)
                {
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                        if (!string.IsNullOrEmpty(tc))
                        {
                            request.Headers.Add("tenantcode", tc);
                            request.Headers.Add("tenant_code", tc);
                        }
                        if (!string.IsNullOrEmpty(_tenantState?.AuthToken))
                        {
                            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tenantState.AuthToken);
                        }

                        var response = await _http.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            var json = await response.Content.ReadAsStringAsync();
                            var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                            // 1. Direct array deserialization (matches UomMaster.razor)
                            try
                            {
                                var direct = System.Text.Json.JsonSerializer.Deserialize<List<UomMasterModel>>(json, opts);
                                if (direct != null && direct.Any())
                                    return direct
                                        .Where(u => !u.deleted && !string.IsNullOrWhiteSpace(u.name))
                                        .OrderBy(u => u.orderno > 0 ? u.orderno : int.MaxValue)
                                        .ThenBy(u => u.name)
                                        .ToList();
                            }
                            catch { }

                            // 2. ValueResponseWrapper ({ "value": [...] })
                            try
                            {
                                var valWrapped = System.Text.Json.JsonSerializer.Deserialize<ValueResponseWrapper>(json, opts);
                                if (valWrapped?.Value != null && valWrapped.Value.Any())
                                {
                                    return valWrapped.Value
                                        .Where(u => !u.deleted && !string.IsNullOrWhiteSpace(u.name))
                                        .OrderBy(u => u.orderno > 0 ? u.orderno : int.MaxValue)
                                        .ThenBy(u => u.name)
                                        .ToList();
                                }
                            }
                            catch { }

                            // 3. ServiceResponseWrapper ({ "data": [...] })
                            try
                            {
                                var wrapped = System.Text.Json.JsonSerializer.Deserialize<ServiceResponseWrapper>(json, opts);
                                if (wrapped?.data != null && wrapped.data.Any())
                                    return wrapped.data
                                        .Where(u => !u.deleted && !string.IsNullOrWhiteSpace(u.name))
                                        .OrderBy(u => u.orderno > 0 ? u.orderno : int.MaxValue)
                                        .ThenBy(u => u.name)
                                        .ToList();
                            }
                            catch { }
                        }
                    }
                    catch { }
                }

                // Fallback: Use InventoryApi named HttpClient (identical to UomMaster.razor)
                try
                {
                    var invClient = _clientFactory.CreateClient("InventoryApi");
                    if (!string.IsNullOrEmpty(tc))
                    {
                        invClient.DefaultRequestHeaders.Remove("tenantcode");
                        invClient.DefaultRequestHeaders.Add("tenantcode", tc);
                    }
                    var json = await invClient.GetStringAsync("ItemMaster/getalluom");
                    var opts = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    
                    try
                    {
                        var direct = System.Text.Json.JsonSerializer.Deserialize<List<UomMasterModel>>(json, opts);
                        if (direct != null && direct.Any())
                            return direct.Where(u => !u.deleted && !string.IsNullOrWhiteSpace(u.name))
                                .OrderBy(u => u.orderno > 0 ? u.orderno : int.MaxValue)
                                .ThenBy(u => u.name).ToList();
                    }
                    catch { }

                    try
                    {
                        var valWrapped = System.Text.Json.JsonSerializer.Deserialize<ValueResponseWrapper>(json, opts);
                        if (valWrapped?.Value != null && valWrapped.Value.Any())
                            return valWrapped.Value.Where(u => !u.deleted && !string.IsNullOrWhiteSpace(u.name))
                                .OrderBy(u => u.orderno > 0 ? u.orderno : int.MaxValue)
                                .ThenBy(u => u.name).ToList();
                    }
                    catch { }
                }
                catch { }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UomMasterService] GetUomMastersAsync failed: {ex.Message}");
            }

            return new List<UomMasterModel>();
        }

        // Helper wrapper for the api/ItemMaster/getalluom response shape: `{ "value": [...], "Count": 3 }`
        private class ValueResponseWrapper
        {
            public List<UomMasterModel>? Value { get; set; }
        }

        // Helper wrapper for ServiceResponse<List<UomMasterModel>> JSON shape
        private class ServiceResponseWrapper
        {
            public List<UomMasterModel>? data { get; set; }
        }


        public async Task<bool> InsertUomMasterAsync(UomMasterModel uom)
        {
            var response = await _http.PostAsJsonAsync("api/UomMaster/insert", uom);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateUomMasterAsync(UomMasterModel uom)
        {
            var response = await _http.PostAsJsonAsync("api/UomMaster/update", uom);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteUomMasterAsync(decimal ucode)
        {
            var response = await _http.GetAsync($"api/UomMaster/delete?ucode={ucode}");
            return response.IsSuccessStatusCode;
        }
    }
}