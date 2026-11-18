using System.Net.Http.Json;
using LIMS_Backend.Model;

namespace LabCare.Services
{
    public class CashVoucherService
    {
        private readonly HttpClient _http;
        private readonly TenantSessionState _session;

        public CashVoucherService(IHttpClientFactory clientFactory, TenantSessionState session)
        {
            _http = clientFactory.CreateClient("LabCareApi");
            _session = session;
        }

        private void ConfigureHeaders()
        {
            if (_http.DefaultRequestHeaders.Contains("tenant_code"))
                _http.DefaultRequestHeaders.Remove("tenant_code");
            _http.DefaultRequestHeaders.Add("tenant_code", _session.TenantCode);

            if (_http.DefaultRequestHeaders.Contains("Authorization"))
                _http.DefaultRequestHeaders.Remove("Authorization");
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {_session.AuthToken}");

            if (_session.BranchCode.HasValue)
            {
                if (_http.DefaultRequestHeaders.Contains("bhcode"))
                    _http.DefaultRequestHeaders.Remove("bhcode");
                _http.DefaultRequestHeaders.Add("bhcode", _session.BranchCode.Value.ToString());
            }
        }

        private async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, string operation)
        {
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                throw new Exception($"{operation} failed. Status: {response.StatusCode}, Details: {content}");
            }
        }

        public async Task<List<CashVoucher>> GetAllAsync(int? bhcode = null)
        {
            ConfigureHeaders();
            
            // Prioritize passed bhcode, fallback to session branch code
            int? activeBhCode = bhcode ?? _session.BranchCode;

            string url = "cashvoucher/getall";
            if (activeBhCode.HasValue) url += $"?bhcode={activeBhCode.Value}";
            
            try 
            { 
                var response = await _http.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
                    
                    // The API returns { "status": "success", "data": { "data": [...] } }
                    if (json.TryGetProperty("data", out var outerData) && 
                        outerData.TryGetProperty("data", out var innerData))
                    {
                        return System.Text.Json.JsonSerializer.Deserialize<List<CashVoucher>>(innerData.GetRawText()) ?? new();
                    }
                    
                    // Fallback: If it's a direct list
                    if (json.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        return System.Text.Json.JsonSerializer.Deserialize<List<CashVoucher>>(json.GetRawText()) ?? new();
                    }
                }
                return new(); 
            }
            catch (Exception ex)
            { 
                Console.WriteLine($"Error in GetAllAsync: {ex.Message}");
                return new(); 
            }
        }

        public async Task<bool> InsertAsync(CashVoucher model)
        {
            ConfigureHeaders();
            model.tenant_code ??= _session.TenantCode;
            var response = await _http.PostAsJsonAsync("cashvoucher/insert", model);
            await EnsureSuccessOrThrowAsync(response, "Insert Cash Voucher");
            return true;
        }

        public async Task<bool> UpdateAsync(CashVoucher model)
        {
            ConfigureHeaders();
            model.tenant_code ??= _session.TenantCode;
            var response = await _http.PostAsJsonAsync("cashvoucher/update", model);
            await EnsureSuccessOrThrowAsync(response, "Update Cash Voucher");
            return true;
        }

        public async Task<bool> SoftDeleteAsync(Guid cashvoucherid)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"cashvoucher/softdelete?cashvoucherguid={cashvoucherid}");
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> HardDeleteAsync(Guid cashvoucherid)
        {
            ConfigureHeaders();
            var response = await _http.DeleteAsync($"cashvoucher/delete?cashvoucherguid={cashvoucherid}");
            return response.IsSuccessStatusCode;
        }
    }
}
