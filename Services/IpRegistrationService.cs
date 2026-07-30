using System.Net.Http.Json;
using medico_backend.Model;

namespace Booking.Services
{
    public class IpRegistrationService
    {
        private readonly HttpClient _http;

        public IpRegistrationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<IPRegistrationModel.IpRegistrationModel>> GetIpRegistrationsAsync(string status)
        {
            return await FetchIpListAsync($"api/IpRegistration/get?ip_status={status}");
        }

        public async Task<List<IPRegistrationModel.IpRegistrationModel>> GetActiveAdmissionsAsync()
        {
            return await FetchIpListAsync("api/IpRegistration/active-admissions");
        }

        private async Task<List<IPRegistrationModel.IpRegistrationModel>> FetchIpListAsync(string url)
        {
            try
            {
                var rawJson = await _http.GetStringAsync(url);
                if (string.IsNullOrWhiteSpace(rawJson))
                    return new List<IPRegistrationModel.IpRegistrationModel>();

                using var doc = System.Text.Json.JsonDocument.Parse(rawJson);
                var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    return System.Text.Json.JsonSerializer.Deserialize<List<IPRegistrationModel.IpRegistrationModel>>(rawJson, options)
                           ?? new List<IPRegistrationModel.IpRegistrationModel>();
                }
                else if (doc.RootElement.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (doc.RootElement.TryGetProperty("value", out var valueProp) && valueProp.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        return System.Text.Json.JsonSerializer.Deserialize<List<IPRegistrationModel.IpRegistrationModel>>(valueProp.GetRawText(), options)
                               ?? new List<IPRegistrationModel.IpRegistrationModel>();
                    }

                    var wrapper = System.Text.Json.JsonSerializer.Deserialize<IpListResponse>(rawJson, options);
                    return wrapper?.value ?? new List<IPRegistrationModel.IpRegistrationModel>();
                }

                return new List<IPRegistrationModel.IpRegistrationModel>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching IP list from {url}: {ex.Message}");
                return new List<IPRegistrationModel.IpRegistrationModel>();
            }
        }

        private class IpListResponse
        {
            public List<IPRegistrationModel.IpRegistrationModel>? value { get; set; }
        }

        public async Task<IPRegistrationModel.IpRegistrationModel?> GetIpRegistrationByIdAsync(Guid ipId)
        {
            try
            {
                var response = await _http.GetFromJsonAsync<IPRegistrationModel.IpRegistrationModel>($"api/IpRegistration/get-by-id?ip_id={ipId}");
                return response;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> AdmitPatientAsync(IPRegistrationModel.CreateIpRegistrationRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/IpRegistration/admit", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<(bool Success, string Message)> DischargePatientAsync(IPRegistrationModel.DischargeRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/IpRegistration/discharge", request);
                var rawContent = await response.Content.ReadAsStringAsync();
                var cleaned = rawContent?.Trim().Trim('"') ?? "";

                if (!response.IsSuccessStatusCode)
                {
                    return (false, string.IsNullOrWhiteSpace(cleaned) ? "Failed to discharge patient." : cleaned);
                }

                if (!string.IsNullOrWhiteSpace(cleaned))
                {
                    if (cleaned.StartsWith("Cannot discharge", StringComparison.OrdinalIgnoreCase) ||
                        cleaned.StartsWith("Error", StringComparison.OrdinalIgnoreCase) ||
                        cleaned.StartsWith("Failed", StringComparison.OrdinalIgnoreCase) ||
                        cleaned.Contains("unbilled charges are pending", StringComparison.OrdinalIgnoreCase) ||
                        cleaned.Contains("Generate the final bill first", StringComparison.OrdinalIgnoreCase))
                    {
                        return (false, cleaned);
                    }
                }

                return (true, string.IsNullOrWhiteSpace(cleaned) ? "Patient discharged successfully!" : cleaned);
            }
            catch (Exception ex)
            {
                return (false, $"Error discharging patient: {ex.Message}");
            }
        }

        public async Task<bool> UpdateIpRegistrationAsync(IPRegistrationModel.UpdateIpRegistrationRequest request, decimal? custid = null)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(request);
            var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json) ?? new();
            if (custid.HasValue)
            {
                dict["custid"] = custid.Value;
            }
            var response = await _http.PostAsJsonAsync("api/IpRegistration/update", dict);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> CancelAdmissionAsync(IPRegistrationModel.CancelAdmissionRequest request)
        {
            var response = await _http.PostAsJsonAsync("api/IpRegistration/cancel", request);
            return response.IsSuccessStatusCode;
        }
    }
}
