using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Medico_Backend.Model;

namespace Booking.Services
{
    public class MasterTenantServices
    {
        private readonly HttpClient _http;

        public MasterTenantServices(HttpClient http)
        {
            _http = http;
        }

        public async Task<TenantReportMethodModel?> GetTenantReportMethodAsync(string tenantCode)
        {
            try
            {
                if (_http.DefaultRequestHeaders.Contains("tenant_code"))
                {
                    _http.DefaultRequestHeaders.Remove("tenant_code");
                }
                _http.DefaultRequestHeaders.Add("tenant_code", tenantCode);

                var response = await _http.GetAsync("api/TenantReportMethod/get");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        return Newtonsoft.Json.JsonConvert.DeserializeObject<TenantReportMethodModel>(content);
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching tenant report method settings: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> InsertTenantReportMethodAsync(TenantReportMethodModel model, string tenantCode)
        {
            try
            {
                if (_http.DefaultRequestHeaders.Contains("tenant_code"))
                {
                    _http.DefaultRequestHeaders.Remove("tenant_code");
                }
                _http.DefaultRequestHeaders.Add("tenant_code", tenantCode);

                var response = await _http.PostAsJsonAsync("api/TenantReportMethod/insert", model);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inserting tenant report method: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateTenantReportMethodAsync(TenantReportMethodModel model, string tenantCode)
        {
            try
            {
                if (_http.DefaultRequestHeaders.Contains("tenant_code"))
                {
                    _http.DefaultRequestHeaders.Remove("tenant_code");
                }
                _http.DefaultRequestHeaders.Add("tenant_code", tenantCode);

                var response = await _http.PostAsJsonAsync("api/TenantReportMethod/update", model);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating tenant report method: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteTenantReportMethodAsync(int id, string tenantCode)
        {
            try
            {
                if (_http.DefaultRequestHeaders.Contains("tenant_code"))
                {
                    _http.DefaultRequestHeaders.Remove("tenant_code");
                }
                _http.DefaultRequestHeaders.Add("tenant_code", tenantCode);

                var response = await _http.DeleteAsync($"api/TenantReportMethod/delete?id={id}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting tenant report method: {ex.Message}");
                return false;
            }
        }
    }
}
